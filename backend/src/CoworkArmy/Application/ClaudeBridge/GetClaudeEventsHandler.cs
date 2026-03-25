using CoworkArmy.Application.ClaudeBridge.DTOs;
using CoworkArmy.Domain.ClaudeBridge;

namespace CoworkArmy.Application.ClaudeBridge;

public class GetClaudeEventsHandler
{
    private readonly IClaudeBridgeRepository _repo;
    public GetClaudeEventsHandler(IClaudeBridgeRepository repo) => _repo = repo;

    public async Task<List<ClaudeEventResponseDto>> HandleAsync(int limit = 50)
    {
        var events = await _repo.GetRecentEventsAsync(limit);
        return events.Select(e => new ClaudeEventResponseDto(
            e.Id, e.Tool, e.FilePath, e.AgentId, e.TaskId, e.Summary, e.CreatedAt)).ToList();
    }
}
