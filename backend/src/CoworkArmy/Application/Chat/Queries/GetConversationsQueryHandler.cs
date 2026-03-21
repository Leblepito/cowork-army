using CoworkArmy.Application.Chat.DTOs;
using CoworkArmy.Domain.Chat;

namespace CoworkArmy.Application.Chat.Queries;

public class GetConversationsQueryHandler
{
    private readonly IChatRepository _repo;
    public GetConversationsQueryHandler(IChatRepository repo) => _repo = repo;

    public async Task<List<ConversationDto>> HandleByAgentAsync(string agentId, int limit = 20)
    {
        var convs = await _repo.GetByAgentAsync(agentId, limit);
        return convs.Select(c => new ConversationDto(
            c.Id, c.AgentId, c.Title,
            c.Messages.Select(m => new ChatMessageResponseDto(m.Id, m.Role, m.Content, m.Timestamp)).ToList(),
            c.UpdatedAt
        )).ToList();
    }

    public async Task<ConversationDto?> HandleByIdAsync(string convId)
    {
        var c = await _repo.GetByIdAsync(convId);
        if (c == null) return null;
        return new ConversationDto(
            c.Id, c.AgentId, c.Title,
            c.Messages.Select(m => new ChatMessageResponseDto(m.Id, m.Role, m.Content, m.Timestamp)).ToList(),
            c.UpdatedAt
        );
    }
}
