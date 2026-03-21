using CoworkArmy.Domain.Common;

namespace CoworkArmy.Domain.Chat;

public class ChatConversation : AggregateRoot
{
    public string AgentId { get; private set; } = "";
    public string Title { get; private set; } = "";
    public List<ChatMessage> Messages { get; private set; } = new();
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private ChatConversation() { }

    public static ChatConversation Create(string agentId, string title = "")
        => new()
        {
            Id = $"conv-{Guid.NewGuid().ToString()[..8]}",
            AgentId = agentId,
            Title = string.IsNullOrEmpty(title) ? $"Sohbet {DateTime.UtcNow:dd MMM HH:mm}" : title
        };

    public void AddMessage(string role, string content)
    {
        Messages.Add(ChatMessage.Create(Id, role, content));
        UpdatedAt = DateTime.UtcNow;
    }
}
