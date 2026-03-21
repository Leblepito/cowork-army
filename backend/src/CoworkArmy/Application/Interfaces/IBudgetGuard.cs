namespace CoworkArmy.Application.Interfaces;

public interface IBudgetGuard
{
    Task<bool> CanSpendAsync(string agentId);
    Task RecordUsageAsync(string agentId, string provider, string model, int inputTokens, int outputTokens, double costUsd, string? taskId = null);
    Task<BudgetStatus> GetStatusAsync();
    bool CheckRateLimit(string agentId, string toolName);
}

public record BudgetStatus(
    double GlobalTodayUsd, double GlobalCapUsd,
    Dictionary<string, double> DeptTodayUsd, double DeptCapUsd,
    Dictionary<string, double> AgentHourUsd, double AgentCapUsd);
