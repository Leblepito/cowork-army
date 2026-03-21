using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.LLM;

public class BudgetGuard : IBudgetGuard
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<BudgetGuard> _log;
    private double _agentHourCap = 0.50;
    private double _deptDayCap = 5.00;
    private double _globalDayCap = 25.00;
    private readonly ConcurrentDictionary<string, List<DateTime>> _rateLimits = new();
    private DateTime _lastCleanup = DateTime.UtcNow;
    private readonly object _cleanupLock = new();

    public BudgetGuard(IServiceProvider sp, ILogger<BudgetGuard> log)
    {
        _sp = sp; _log = log;
    }

    public async Task<bool> CanSpendAsync(string agentId)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoworkArmy.Infrastructure.Persistence.CoworkDbContext>();
        var now = DateTime.UtcNow;
        var hourAgo = now.AddHours(-1);
        var todayStart = now.Date;

        var agentHour = await db.LlmUsage
            .Where(u => u.AgentId == agentId && u.Timestamp >= hourAgo)
            .Select(u => (double?)u.CostUsd).SumAsync() ?? 0;
        if (agentHour >= _agentHourCap)
        {
            _log.LogWarning("Budget exceeded: agent {AgentId} ${Cost:F2}/hr (cap ${Cap:F2})", agentId, agentHour, _agentHourCap);
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
            await notifier.SendBudgetWarningAsync("agent_hour", (decimal)agentHour, (decimal)_agentHourCap);
            return false;
        }

        var globalToday = await db.LlmUsage
            .Where(u => u.Timestamp >= todayStart)
            .Select(u => (double?)u.CostUsd).SumAsync() ?? 0;
        if (globalToday >= _globalDayCap)
        {
            _log.LogWarning("Budget exceeded: global ${Cost:F2}/day (cap ${Cap:F2})", globalToday, _globalDayCap);
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
            await notifier.SendBudgetWarningAsync("global_day", (decimal)globalToday, (decimal)_globalDayCap);
            return false;
        }
        return true;
    }

    public async Task RecordUsageAsync(string agentId, string provider, string model, int inputTokens, int outputTokens, double costUsd, string? taskId = null)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoworkArmy.Infrastructure.Persistence.CoworkDbContext>();
        db.LlmUsage.Add(new CoworkArmy.Domain.LlmUsageEntry
        {
            AgentId = agentId, Provider = provider, Model = model,
            InputTokens = inputTokens, OutputTokens = outputTokens,
            CostUsd = costUsd, TaskId = taskId, Timestamp = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task<BudgetStatus> GetStatusAsync()
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoworkArmy.Infrastructure.Persistence.CoworkDbContext>();
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var hourAgo = now.AddHours(-1);

        var todayUsage = await db.LlmUsage.Where(u => u.Timestamp >= todayStart).ToListAsync();
        var globalToday = todayUsage.Sum(u => u.CostUsd);
        var deptToday = todayUsage.GroupBy(u => u.AgentId).ToDictionary(g => g.Key, g => g.Sum(u => u.CostUsd));
        var hourUsage = await db.LlmUsage.Where(u => u.Timestamp >= hourAgo).ToListAsync();
        var agentHour = hourUsage.GroupBy(u => u.AgentId).ToDictionary(g => g.Key, g => g.Sum(u => u.CostUsd));

        return new BudgetStatus(globalToday, _globalDayCap, deptToday, _deptDayCap, agentHour, _agentHourCap);
    }

    public bool CheckRateLimit(string agentId, string toolName)
    {
        if (DateTime.UtcNow - _lastCleanup > TimeSpan.FromHours(1))
        {
            lock (_cleanupLock)
            {
                if (DateTime.UtcNow - _lastCleanup > TimeSpan.FromHours(1))
                {
                    _lastCleanup = DateTime.UtcNow;
                    var staleKeys = _rateLimits
                        .Where(kv => kv.Value.All(t => (DateTime.UtcNow - t).TotalMinutes > 1))
                        .Select(kv => kv.Key)
                        .ToList();
                    foreach (var key in staleKeys)
                        _rateLimits.TryRemove(key, out _);
                }
            }
        }

        var k = $"{agentId}:{toolName}";
        var list = _rateLimits.GetOrAdd(k, _ => new List<DateTime>());
        var now = DateTime.UtcNow;
        lock (list)
        {
            list.RemoveAll(t => (now - t).TotalMinutes >= 1);
            if (list.Count >= 10) return false;
            list.Add(now);
        }
        return true;
    }
}
