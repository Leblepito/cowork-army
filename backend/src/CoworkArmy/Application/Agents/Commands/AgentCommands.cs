using CoworkArmy.Application.Agents.DTOs;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Common;
using CoworkArmy.Domain.Events;

namespace CoworkArmy.Application.Agents.Commands;

// ═══ SPAWN ═══
public record SpawnAgentCommand(string AgentId, string? Task = null);

public class SpawnAgentCommandHandler
{
    private readonly IAgentRepository _repo;
    private readonly IRealtimeNotifier _notifier;
    private readonly IStatusTracker _tracker;
    private readonly IEventRepository _events;

    public SpawnAgentCommandHandler(
        IAgentRepository repo, IRealtimeNotifier notifier,
        IStatusTracker tracker, IEventRepository events)
    {
        _repo = repo; _notifier = notifier;
        _tracker = tracker; _events = events;
    }

    public async Task<AgentStatusDto> HandleAsync(SpawnAgentCommand cmd)
    {
        var agent = await _repo.GetByIdAsync(cmd.AgentId)
            ?? throw new DomainException($"Agent not found: {cmd.AgentId}");

        var taskDesc = cmd.Task ?? "manual spawn";
        _tracker.Set(agent.Id, "working", $"▶ {taskDesc}");
        await _notifier.SendStatusChangeAsync(agent.Id, "working");
        await _notifier.SendEventAsync("spawn", agent.Id, $"{agent.Icon} {agent.Name} spawned: {taskDesc}");
        await _events.AddAsync(new AgentEvent { Type = "spawn", AgentId = agent.Id, Message = $"Spawned: {taskDesc}" });

        // Simulate work then auto-idle
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000);
            _tracker.Set(agent.Id, "idle", "✅ Done");
            await _notifier.SendStatusChangeAsync(agent.Id, "idle");
        });

        var st = _tracker.Get(agent.Id);
        return new AgentStatusDto(st.AgentId, st.Status, st.Alive, st.Lines, st.StartedAt);
    }
}

// ═══ KILL ═══
public record KillAgentCommand(string AgentId);

public class KillAgentCommandHandler
{
    private readonly IAgentRepository _repo;
    private readonly IRealtimeNotifier _notifier;
    private readonly IStatusTracker _tracker;

    public KillAgentCommandHandler(
        IAgentRepository repo, IRealtimeNotifier notifier, IStatusTracker tracker)
    {
        _repo = repo; _notifier = notifier; _tracker = tracker;
    }

    public async Task HandleAsync(KillAgentCommand cmd)
    {
        var agent = await _repo.GetByIdAsync(cmd.AgentId)
            ?? throw new DomainException($"Agent not found: {cmd.AgentId}");

        if (agent.IsImmortal)
            throw new DomainException($"Cannot kill immortal agent: {agent.Name}");

        _tracker.Set(cmd.AgentId, "idle", "⏹ Killed");
        await _notifier.SendStatusChangeAsync(cmd.AgentId, "idle");
    }
}

// ═══ CREATE ═══
public record CreateAgentCommand(AgentCreateDto Data);

public class CreateAgentCommandHandler
{
    private readonly IAgentRepository _repo;

    public CreateAgentCommandHandler(IAgentRepository repo) => _repo = repo;

    public async Task<AgentResponseDto> HandleAsync(CreateAgentCommand cmd)
    {
        var d = cmd.Data;
        var tier = Enum.Parse<AgentTier>(d.Tier, true);
        var agent = Agent.Create(
            $"dyn-{Guid.NewGuid().ToString()[..6]}",
            d.Name, d.Icon, tier, d.Color,
            d.Department, d.Description, d.Skills, d.SystemPrompt, false);

        await _repo.AddAsync(agent);

        return new AgentResponseDto(
            agent.Id, agent.Name, agent.Icon, agent.Tier.ToString(),
            agent.Color, agent.Department, agent.Description,
            agent.Skills, agent.IsBase, agent.CreatedAt);
    }
}
