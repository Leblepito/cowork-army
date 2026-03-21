namespace CoworkArmy.Domain.Chat;

public class ChatMessage
{
    public string Id { get; private set; } = "";
    public string ConversationId { get; private set; } = "";
    public string Role { get; private set; } = "user"; // system, user, assistant
    public string Content { get; private set; } = "";
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private ChatMessage() { }

    public static ChatMessage Create(string conversationId, string role, string content)
        => new()
        {
            Id = $"msg-{Guid.NewGuid().ToString()[..8]}",
            ConversationId = conversationId,
            Role = role,
            Content = content
        };
}
