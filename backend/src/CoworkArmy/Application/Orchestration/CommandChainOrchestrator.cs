using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.Messages;
using CoworkArmy.Domain.Tasks;

namespace CoworkArmy.Application.Orchestration;

public class CommandChainOrchestrator
{
    private readonly IAgentRepository _agents;
    private readonly ITaskRepository _tasks;
    private readonly IEventRepository _events;
    private readonly IRealtimeNotifier _notifier;
    private readonly IStatusTracker _tracker;
    private readonly AgentOrchestrator _orchestrator;
    private readonly IMessageBus _bus;
    private readonly ILogger<CommandChainOrchestrator> _log;

    public CommandChainOrchestrator(
        IAgentRepository agents, ITaskRepository tasks, IEventRepository events,
        IRealtimeNotifier notifier, IStatusTracker tracker,
        AgentOrchestrator orchestrator, IMessageBus bus,
        ILogger<CommandChainOrchestrator> log)
    {
        _agents = agents; _tasks = tasks; _events = events;
        _notifier = notifier; _tracker = tracker;
        _orchestrator = orchestrator; _bus = bus; _log = log;
    }

    public async Task ExecuteAsync(string ceoMessage, CancellationToken ct = default)
    {
        _log.LogInformation("Command chain started: {Message}", ceoMessage);

        // Phase 1: CEO decides which director
        var ceoResponse = await _orchestrator.ThinkAndActAsync("ceo", $"""
            You received this order: "{ceoMessage}"

            Available directors:
            - trade-master: Trading (chart analysis, risk, quant)
            - clinic-dir: Medical (patient care)
            - hotel-mgr: Hotel (travel, concierge)
            - tech-lead: Software (full-stack, data-ops, debugging)
            - hr-agent: HR (agent management)

            Respond with exactly:
            DIRECTOR: <director-id>
            INSTRUCTION: <what to tell the director>
            """, ct);

        var (directorId, instruction) = ParseDirectorAssignment(ceoResponse);
        if (directorId == null)
        {
            _log.LogWarning("CEO couldn't assign: {Message}", ceoMessage);
            await _events.AddAsync(new AgentEvent { Type = "error", AgentId = "ceo", Message = $"Failed to assign: {ceoMessage}" });
            return;
        }

        await _notifier.SendCommandAsync("ceo_to_dir", "ceo", directorId, instruction);
        await _events.AddAsync(new AgentEvent { Type = "command", AgentId = "ceo", Message = $"CEO → {directorId}: {instruction[..Math.Min(60, instruction.Length)]}" });

        // Phase 2: Director breaks into tasks
        var dirResponse = await _orchestrator.ThinkAndActAsync(directorId, $"""
            CEO instructed: "{instruction}"

            Break this into specific tasks for your workers.
            Respond with one line per task:
            TASK: <worker-id> | <task description>
            """, ct);

        var workerTasks = ParseWorkerTasks(dirResponse);
        if (workerTasks.Count == 0)
        {
            _log.LogWarning("Director {Dir} produced no tasks", directorId);
            return;
        }

        await _notifier.SendCommandAsync("dir_to_workers", directorId, "team",
            $"Distributing {workerTasks.Count} tasks");

        // Phase 3: Workers execute in parallel
        var jobs = workerTasks.Select(async wt =>
        {
            var task = AgentTask.Create(wt.Description, wt.WorkerId, directorId, TaskPriority.High,
                $"Chain: {ceoMessage}");
            task.Start();
            await _tasks.AddAsync(task);
            await _notifier.SendCommandAsync("dir_to_worker", directorId, wt.WorkerId, wt.Description);

            try
            {
                var result = await _orchestrator.ThinkAndActAsync(wt.WorkerId, wt.Description, ct);
                task.Complete();
                await _tasks.UpdateAsync(task);
                await _notifier.SendEventAsync("complete", wt.WorkerId,
                    $"✅ {wt.Description[..Math.Min(30, wt.Description.Length)]}");
                return (wt.WorkerId, true, result);
            }
            catch (Exception ex)
            {
                task.Fail(ex.Message);
                await _tasks.UpdateAsync(task);
                return (wt.WorkerId, false, ex.Message);
            }
        });

        var results = await Task.WhenAll(jobs);

        // Phase 4: Summary back to CEO
        var summary = string.Join("\n", results.Select(r =>
            $"- {r.Item1}: {(r.Item2 ? "✅" : "❌")} {r.Item3[..Math.Min(60, r.Item3.Length)]}"));

        await _bus.SendAsync(new AgentMessage
        {
            FromId = directorId, ToId = "ceo",
            Type = MessageType.Response,
            Content = $"Completed. Results:\n{summary}",
        });

        _log.LogInformation("Chain completed: {Message}", ceoMessage);
    }

    private (string? DirectorId, string Instruction) ParseDirectorAssignment(string response)
    {
        string? dirId = null; var instruction = "";
        foreach (var line in response.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("DIRECTOR:", StringComparison.OrdinalIgnoreCase))
                dirId = trimmed.Split(':', 2)[1].Trim().ToLower();
            if (trimmed.StartsWith("INSTRUCTION:", StringComparison.OrdinalIgnoreCase))
                instruction = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
        }
        return (dirId, instruction);
    }

    private List<(string WorkerId, string Description)> ParseWorkerTasks(string response)
    {
        var tasks = new List<(string, string)>();
        foreach (var line in response.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("TASK:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = trimmed[(trimmed.IndexOf(':') + 1)..].Split('|', 2);
                if (parts.Length == 2) tasks.Add((parts[0].Trim().ToLower(), parts[1].Trim()));
            }
        }
        return tasks;
    }
}
