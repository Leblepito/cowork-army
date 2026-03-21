namespace CoworkArmy.Domain;

public class AgentPerformance
{
    public string AgentId { get; set; } = "";
    public int TasksCompleted { get; set; }
    public int TasksFailed { get; set; }
    public double AvgResponseMs { get; set; }
    public long TotalTokens { get; set; }
    public double EstimatedCost { get; set; }
    public int Warnings { get; set; }
    public string Grade { get; set; } = "B";
    public DateTime? LastActiveAt { get; set; }
}
