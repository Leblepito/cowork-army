using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.Common;

namespace CoworkArmy.Application.HR;

public class HRRetireHandler
{
    private readonly IAgentRepository _agents;
    private readonly IRealtimeNotifier _notifier;
    private readonly IEventRepository _events;
    private readonly ILogger<HRRetireHandler> _log;

    public HRRetireHandler(IAgentRepository agents, IRealtimeNotifier notifier,
        IEventRepository events, ILogger<HRRetireHandler> log)
    { _agents = agents; _notifier = notifier; _events = events; _log = log; }

    public async Task HandleAsync(string agentId, string reason)
    {
        var agent = await _agents.GetByIdAsync(agentId)
            ?? throw new DomainException($"Agent not found: {agentId}");

        if (agent.IsImmortal)
            throw new DomainException($"Cannot retire immortal agent: {agent.Name}");

        agent.Retire();
        await _agents.UpdateAsync(agent);

        await _notifier.SendEventAsync("kill", "hr-agent", $"HR retired: {agent.Icon} {agent.Name} — {reason}");
        await _notifier.SendAgentRetiredAsync(agentId, reason);
        await _events.AddAsync(new AgentEvent { Type = "kill", AgentId = agentId, Message = $"Retired: {reason}" });

        _log.LogInformation("HR retired agent: {Id} {Name} — {Reason}", agentId, agent.Name, reason);
    }
}
