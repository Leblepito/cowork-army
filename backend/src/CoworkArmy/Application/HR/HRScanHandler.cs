using CoworkArmy.Domain.HR;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Application.HR;

public class HRScanHandler
{
    private readonly IPerformanceRepository _perfRepo;
    private readonly IHRProposalRepository _proposalRepo;
    private readonly IAgentRepository _agents;
    private readonly IRealtimeNotifier _notifier;
    private readonly ILogger<HRScanHandler> _log;

    public HRScanHandler(IPerformanceRepository perfRepo, IHRProposalRepository proposalRepo,
        IAgentRepository agents, IRealtimeNotifier notifier, ILogger<HRScanHandler> log)
    { _perfRepo = perfRepo; _proposalRepo = proposalRepo; _agents = agents; _notifier = notifier; _log = log; }

    public async Task<List<HRProposal>> ScanAsync()
    {
        var proposals = new List<HRProposal>();
        var performances = await _perfRepo.GetAllAsync();
        var agents = await _agents.GetAllAsync();
        var activeAgents = agents.Where(a => a.IsActive).ToList();

        foreach (var perf in performances)
        {
            var agent = activeAgents.FirstOrDefault(a => a.Id == perf.AgentId);
            if (agent == null || agent.IsImmortal) continue;

            if (perf.TasksFailed >= HRRules.ConsecutiveFailuresForWarning && perf.Warnings < HRRules.WarningsForRetire)
            {
                perf.Warnings++;
                perf.TasksFailed = 0;
                await _notifier.SendEventAsync("info", "hr-agent", $"Warning #{perf.Warnings} for {agent.Icon} {agent.Name}");

                if (perf.Warnings >= HRRules.WarningsForRetire)
                {
                    agent.Retire();
                    await _agents.UpdateAsync(agent);
                    await _notifier.SendEventAsync("kill", "hr-agent", $"Auto-retired: {agent.Icon} {agent.Name} (3 warnings)");
                }
            }

            if (perf.LastActiveAt.HasValue && (DateTime.UtcNow - perf.LastActiveAt.Value).TotalDays > HRRules.IdleDaysForReview)
            {
                if (!await _proposalRepo.ExistsPendingAsync(perf.AgentId, ProposalType.Review))
                {
                    proposals.Add(new HRProposal
                    {
                        Type = ProposalType.Review,
                        AgentId = perf.AgentId,
                        Reason = $"{agent.Name} idle for {(DateTime.UtcNow - perf.LastActiveAt.Value).Days} days",
                    });
                }
            }

            var deptPerfs = performances.Where(p => activeAgents.Any(a => a.Id == p.AgentId && a.Department == agent.Department));
            var deptAvgCost = deptPerfs.Any() ? deptPerfs.Average(p => p.EstimatedCost) : 0;
            if (deptAvgCost > 0 && perf.EstimatedCost > deptAvgCost * HRRules.CostMultiplierForWarning)
            {
                await _notifier.SendEventAsync("info", "hr-agent",
                    $"Cost warning: {agent.Name} ${perf.EstimatedCost:F2} vs dept avg ${deptAvgCost:F2}");
            }
        }

        var nonImmortal = activeAgents.Where(a => !a.IsImmortal).ToList();
        for (var i = 0; i < nonImmortal.Count; i++)
        {
            for (var j = i + 1; j < nonImmortal.Count; j++)
            {
                var a1 = nonImmortal[i]; var a2 = nonImmortal[j];
                if (a1.Department != a2.Department || a1.Skills != a2.Skills) continue;

                var p1 = performances.FirstOrDefault(p => p.AgentId == a1.Id);
                var p2 = performances.FirstOrDefault(p => p.AgentId == a2.Id);
                var idle1 = p1?.LastActiveAt != null && (DateTime.UtcNow - p1.LastActiveAt.Value).TotalDays > HRRules.IdleDaysForDuplicateRetire;
                var idle2 = p2?.LastActiveAt != null && (DateTime.UtcNow - p2.LastActiveAt.Value).TotalDays > HRRules.IdleDaysForDuplicateRetire;
                if (idle1 || idle2)
                {
                    var idleAgent = idle1 ? a1 : a2;
                    if (!await _proposalRepo.ExistsPendingAsync(idleAgent.Id, ProposalType.Retire))
                    {
                        proposals.Add(new HRProposal
                        {
                            Type = ProposalType.Retire,
                            AgentId = idleAgent.Id,
                            Reason = $"Duplicate skills with {(idle1 ? a2.Name : a1.Name)}, idle >{HRRules.IdleDaysForDuplicateRetire} days",
                        });
                    }
                }
            }
        }

        foreach (var perf in performances)
        {
            var total = perf.TasksCompleted + perf.TasksFailed;
            if (total == 0) { perf.Grade = "B"; continue; }
            var successRate = (double)perf.TasksCompleted / total;
            perf.Grade = successRate switch
            {
                >= 0.9 => "A", >= 0.7 => "B", >= 0.5 => "C", >= 0.3 => "D", _ => "F",
            };
        }

        // Save performance changes (warnings + grades)
        foreach (var perf in performances)
            await _perfRepo.UpdateAsync(perf);

        foreach (var p in proposals) await _proposalRepo.AddAsync(p);

        _log.LogInformation("HR scan complete: {Count} proposals", proposals.Count);
        return proposals;
    }
}
