using System.Text.Json;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.Tasks;
using CoworkArmy.Domain.Tools;

namespace CoworkArmy.Application.Orchestration;

public class AgentOrchestrator
{
    private readonly IAgentRepository _agents;
    private readonly ILlmProviderFactory _llmFactory;
    private readonly IBudgetGuard _budget;
    private readonly IRealtimeNotifier _notifier;
    private readonly IEventRepository _events;
    private readonly IStatusTracker _tracker;
    private readonly ToolRegistry _tools;
    private readonly ILogger<AgentOrchestrator> _log;

    public AgentOrchestrator(
        IAgentRepository agents, ILlmProviderFactory llmFactory, IBudgetGuard budget,
        IRealtimeNotifier notifier, IEventRepository events,
        IStatusTracker tracker, ToolRegistry tools,
        ILogger<AgentOrchestrator> log)
    {
        _agents = agents; _llmFactory = llmFactory; _budget = budget;
        _notifier = notifier; _events = events;
        _tracker = tracker; _tools = tools; _log = log;
    }

    public async Task<string> ThinkAndActAsync(string agentId, string task, CancellationToken ct = default)
    {
        var agent = await _agents.GetByIdAsync(agentId)
            ?? throw new Exception($"Agent not found: {agentId}");

        if (!await _budget.CanSpendAsync(agentId))
            return "Budget cap exceeded — task queued.";

        var llm = _llmFactory.GetByTier(agent.Tier.ToString());
        var model = llm.Name switch
        {
            "anthropic" => agent.Tier >= AgentTier.DIR ? "claude-sonnet-4-6" : "claude-haiku-4-5",
            "gemini" => "gemini-2.5-flash",
            "openai" => agent.Tier >= AgentTier.DIR ? "gpt-4o" : "gpt-4o-mini",
            _ => "gemini-2.5-flash"
        };

        var agentTools = _tools.GetForAgent(Array.Empty<string>());
        var toolDescs = agentTools.Select(t =>
            $"- {t.Name}: {t.Description} (params: {string.Join(", ", t.RequiredParams)})").ToList();

        var jsonExample = """{"tool": "tool_name", "params": {"key": "value"}}""";
        var systemPrompt = $"""
            {agent.SystemPrompt}

            Available tools:
            {string.Join("\n", toolDescs)}

            To use a tool, respond ONLY with JSON: {jsonExample}
            To give a final answer, respond normally without JSON.
            Think step by step. Use tools when needed.
            """;

        _tracker.Set(agentId, "thinking", $"▶ {task}");
        await _notifier.SendStatusChangeAsync(agentId, "thinking");
        await _notifier.SendEventAsync("work", agentId,
            $"{agent.Icon} düşünüyor: {task[..Math.Min(40, task.Length)]}");

        var messages = new List<LlmMessage> { new("user", task) };
        var finalResponse = "";

        for (var i = 0; i < 5; i++)
        {
            var request = new LlmRequest(model, systemPrompt, messages, MaxTokens: 1024);
            var response = await llm.SendAsync(request, ct);

            await _budget.RecordUsageAsync(agentId, llm.Name, model,
                response.InputTokens, response.OutputTokens, response.CostUsd);

            var toolCall = TryParseToolCall(response.Content);
            if (toolCall != null)
            {
                var tool = _tools.Get(toolCall.Value.Name);
                if (tool == null)
                {
                    messages.Add(new("assistant", response.Content));
                    messages.Add(new("user",
                        $"Tool '{toolCall.Value.Name}' not found. Available: {string.Join(", ", agentTools.Select(t => t.Name))}"));
                    continue;
                }

                if (tool.Permission == ToolPermission.Elevated && !_budget.CheckRateLimit(agentId, tool.Name))
                {
                    messages.Add(new("assistant", response.Content));
                    messages.Add(new("user", $"Rate limit exceeded for {tool.Name} (10/min). Wait."));
                    continue;
                }

                _tracker.Set(agentId, "working", $"🔧 {tool.Name}");
                await _notifier.SendStatusChangeAsync(agentId, "working");

                toolCall.Value.Params["_from_agent"] = agentId;

                var timeout = TaskTimeout.ForTool(tool.Name);
                using var toolCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                toolCts.CancelAfter(timeout);

                ToolResult toolResult;
                try { toolResult = await tool.ExecuteAsync(toolCall.Value.Params); }
                catch (OperationCanceledException)
                { toolResult = new ToolResult(false, "", Error: $"Timed out ({timeout.TotalSeconds}s)"); }
                catch (Exception ex)
                { toolResult = new ToolResult(false, "", Error: ex.Message); }

                messages.Add(new("assistant", response.Content));
                messages.Add(new("user",
                    $"Tool result ({tool.Name}): {(toolResult.Success ? toolResult.Output : $"ERROR: {toolResult.Error}")}"));

                await _events.AddAsync(new AgentEvent
                {
                    Type = "work", AgentId = agentId,
                    Message = $"{agent.Icon} tool:{tool.Name} → {(toolResult.Success ? "✅" : "❌")}"
                });
            }
            else
            {
                finalResponse = response.Content;
                break;
            }
        }

        if (string.IsNullOrEmpty(finalResponse))
            finalResponse = messages.LastOrDefault()?.Content ?? "No response";

        _tracker.Set(agentId, "idle", "✅ Done");
        await _notifier.SendStatusChangeAsync(agentId, "idle");
        await _notifier.SendConversationAsync(agentId, agent.Icon, "system",
            finalResponse[..Math.Min(100, finalResponse.Length)]);
        await _events.AddAsync(new AgentEvent
        {
            Type = "complete", AgentId = agentId,
            Message = $"{agent.Icon}: {finalResponse[..Math.Min(80, finalResponse.Length)]}"
        });

        return finalResponse;
    }

    private (string Name, Dictionary<string, string> Params)? TryParseToolCall(string content)
    {
        try
        {
            content = content.Trim();
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start < 0 || end < 0 || end <= start) return null;
            var json = content[start..(end + 1)];
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("tool", out var toolProp)) return null;
            var toolName = toolProp.GetString() ?? "";
            var paramDict = new Dictionary<string, string>();
            if (root.TryGetProperty("params", out var paramsProp))
                foreach (var prop in paramsProp.EnumerateObject())
                    paramDict[prop.Name] = prop.Value.ToString();
            return (toolName, paramDict);
        }
        catch { return null; }
    }
}
