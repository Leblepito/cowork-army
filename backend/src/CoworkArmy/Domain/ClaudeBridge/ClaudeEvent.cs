namespace CoworkArmy.Domain.ClaudeBridge;

public class ClaudeEvent
{
    public int Id { get; private set; }
    public string Tool { get; private set; } = string.Empty;
    public string? FilePath { get; private set; }
    public string AgentId { get; private set; } = string.Empty;
    public string? TaskId { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string Metadata { get; private set; } = "{}";
    public DateTime CreatedAt { get; private set; }

    private ClaudeEvent() { }

    public static ClaudeEvent Create(
        string tool, string agentId, string summary,
        string? filePath = null, string? taskId = null, string metadata = "{}")
    {
        return new ClaudeEvent
        {
            Tool = tool, AgentId = agentId, Summary = summary,
            FilePath = filePath, TaskId = taskId, Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };
    }
}
