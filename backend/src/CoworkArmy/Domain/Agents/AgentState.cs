namespace CoworkArmy.Domain.Agents;

public class AgentState
{
    public string AgentId { get; set; } = "";
    public string Status { get; set; } = "idle";
    public string? CurrentTaskId { get; set; }
    public string ContextSummary { get; set; } = "";
    public string LastMessages { get; set; } = "[]";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
