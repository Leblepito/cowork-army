using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Events;
using CoworkArmy.Application.HR.DTOs;

namespace CoworkArmy.Application.HR;

public class HRSpawnHandler
{
    private readonly IAgentRepository _agents;
    private readonly ILlmProvider _llm;
    private readonly IRealtimeNotifier _notifier;
    private readonly IEventRepository _events;
    private readonly IPerformanceRepository _perfRepo;
    private readonly IAgentStateRepository _stateRepo;
    private readonly ILogger<HRSpawnHandler> _log;

    public HRSpawnHandler(IAgentRepository agents, ILlmProvider llm,
        IRealtimeNotifier notifier, IEventRepository events,
        IPerformanceRepository perfRepo, IAgentStateRepository stateRepo,
        ILogger<HRSpawnHandler> log)
    {
        _agents = agents; _llm = llm; _notifier = notifier;
        _events = events; _perfRepo = perfRepo; _stateRepo = stateRepo; _log = log;
    }

    public async Task<SpawnResultDto> HandleAsync(string reason, string department)
    {
        // Ask HR Agent LLM to design a new agent
        var request = new LlmRequest(
            Model: "claude-sonnet-4-6",
            SystemPrompt: "You are the HR Agent of COWORK.ARMY. Design a new AI agent based on the need described. Respond with EXACTLY this format:\nNAME: <agent name>\nICON: <single emoji>\nTIER: WRK\nDEPARTMENT: <department>\nSKILLS: <comma-separated skills>\nSYSTEM_PROMPT: <one sentence role description>\nTOOLS: <comma-separated tool names from: web_search, file_read, file_write, agent_message, code_execute>",
            Messages: new List<LlmMessage> { new("user", $"We need a new agent because: {reason}\nDepartment: {department}") },
            MaxTokens: 512);

        var response = await _llm.SendAsync(request);
        var parsed = ParseAgentDesign(response.Content);

        // Validate department from LLM output and fallback to engineering if invalid
        var validatedDept = parsed.Department;
        if (!new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "engineering", "marketing", "operations", "finance", "hr", "design", "support", "research"
        }.Contains(validatedDept))
            validatedDept = "engineering";

        // Validate tools against allowlist
        var allowedTools = new HashSet<string> { "web_search", "file_read", "file_write", "agent_message", "code_execute" };
        var validatedTools = parsed.Tools.Where(t => allowedTools.Contains(t)).ToArray();
        if (validatedTools.Length == 0) validatedTools = new[] { "web_search", "file_read", "agent_message" };

        // Sanitize name and system prompt length, apply validated tools
        var designName = parsed.Name.Length > 50 ? parsed.Name[..50] : parsed.Name;
        var designIcon = parsed.Icon;
        var designDept = validatedDept;
        var designSkills = parsed.Skills;
        var designPrompt = parsed.SystemPrompt.Length > 500 ? parsed.SystemPrompt[..500] : parsed.SystemPrompt;
        var designTools = validatedTools;

        var agentId = $"dyn-{Guid.NewGuid().ToString("N")[..6]}";
        var agent = Agent.Create(
            agentId, designName, designIcon, AgentTier.WRK, "#6b7280",
            designDept, designName, $"[\"{string.Join("\",\"", designSkills)}\"]",
            designPrompt, isBase: false, createdBy: "hr-agent",
            tools: $"[\"{string.Join("\",\"", designTools)}\"]");

        await _agents.AddAsync(agent);

        // Create performance tracking row
        await _perfRepo.AddAsync(new AgentPerformance { AgentId = agentId });

        // Create persistent agent state row
        await _stateRepo.AddAsync(new AgentState { AgentId = agentId });

        await _notifier.SendEventAsync("spawn", "hr-agent", $"HR spawned: {designIcon} {designName}");
        await _notifier.SendAgentSpawnedAsync(agentId, designName, designIcon, designDept, "hr-agent");
        await _events.AddAsync(new AgentEvent { Type = "spawn", AgentId = agentId, Message = $"Spawned by HR: {reason}" });

        _log.LogInformation("HR spawned agent: {Id} {Name} for {Dept}", agentId, designName, department);

        return new SpawnResultDto(agentId, designName, designIcon, designDept, "hr-agent");
    }

    private (string Name, string Icon, string Department, string[] Skills, string SystemPrompt, string[] Tools)
        ParseAgentDesign(string response)
    {
        var name = "New Agent"; var icon = "🤖"; var dept = "software";
        var skills = new[] { "general" }; var prompt = "Complete assigned tasks.";
        var tools = new[] { "web_search", "file_read", "agent_message" };

        foreach (var line in response.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("NAME:", StringComparison.OrdinalIgnoreCase))
                name = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
            else if (trimmed.StartsWith("ICON:", StringComparison.OrdinalIgnoreCase))
                icon = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
            else if (trimmed.StartsWith("DEPARTMENT:", StringComparison.OrdinalIgnoreCase))
                dept = trimmed[(trimmed.IndexOf(':') + 1)..].Trim().ToLower();
            else if (trimmed.StartsWith("SKILLS:", StringComparison.OrdinalIgnoreCase))
                skills = trimmed[(trimmed.IndexOf(':') + 1)..].Trim().Split(',').Select(s => s.Trim()).ToArray();
            else if (trimmed.StartsWith("SYSTEM_PROMPT:", StringComparison.OrdinalIgnoreCase))
                prompt = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
            else if (trimmed.StartsWith("TOOLS:", StringComparison.OrdinalIgnoreCase))
                tools = trimmed[(trimmed.IndexOf(':') + 1)..].Trim().Split(',').Select(s => s.Trim()).ToArray();
        }
        return (name, icon, dept, skills, prompt, tools);
    }
}
