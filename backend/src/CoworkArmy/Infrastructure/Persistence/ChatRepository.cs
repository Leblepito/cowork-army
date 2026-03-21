using Microsoft.EntityFrameworkCore;
using CoworkArmy.Domain.Chat;

namespace CoworkArmy.Infrastructure.Persistence;

public class ChatRepository : IChatRepository
{
    private readonly CoworkDbContext _db;
    public ChatRepository(CoworkDbContext db) => _db = db;

    public async Task<ChatConversation?> GetByIdAsync(string id)
        => await _db.ChatConversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<ChatConversation>> GetByAgentAsync(string agentId, int limit = 20)
        => await _db.ChatConversations
            .Where(c => c.AgentId == agentId)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(limit)
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .ToListAsync();

    public async Task AddAsync(ChatConversation conversation)
    {
        _db.ChatConversations.Add(conversation);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ChatConversation conversation)
    {
        _db.ChatConversations.Update(conversation);
        await _db.SaveChangesAsync();
    }
}
