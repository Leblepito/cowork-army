namespace CoworkArmy.Domain;

public class LlmUsageEntry
{
    public int Id { get; set; }
    public string AgentId { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public double CostUsd { get; set; }
    public string? TaskId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
