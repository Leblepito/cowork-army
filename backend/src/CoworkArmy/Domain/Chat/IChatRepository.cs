namespace CoworkArmy.Domain.Chat;

public interface IChatRepository
{
    Task<ChatConversation?> GetByIdAsync(string id);
    Task<List<ChatConversation>> GetByAgentAsync(string agentId, int limit = 20);
    Task AddAsync(ChatConversation conversation);
    Task UpdateAsync(ChatConversation conversation);
}
