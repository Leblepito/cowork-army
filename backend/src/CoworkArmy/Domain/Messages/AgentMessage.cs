namespace CoworkArmy.Domain.Messages;

public record AgentMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public required string FromId { get; init; }
    public required string ToId { get; init; }
    public required MessageType Type { get; init; }
    public required string Content { get; init; }
    public bool RequiresResponse { get; init; }
    public MessagePriority Priority { get; init; } = MessagePriority.Normal;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public enum MessageType { Command, Request, Info, Response }
public enum MessagePriority { Low, Normal, High, Critical }
