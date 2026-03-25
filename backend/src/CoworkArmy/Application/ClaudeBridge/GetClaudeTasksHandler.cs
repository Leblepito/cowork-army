using System.Text.Json;
using CoworkArmy.Application.ClaudeBridge.DTOs;
using CoworkArmy.Domain.ClaudeBridge;

namespace CoworkArmy.Application.ClaudeBridge;

public class GetClaudeTasksHandler
{
    private readonly IClaudeBridgeRepository _repo;
    public GetClaudeTasksHandler(IClaudeBridgeRepository repo) => _repo = repo;

    public async Task<List<ClaudeTaskResponseDto>> HandleAsync()
    {
        var tasks = await _repo.GetActiveTasksAsync();
        return tasks.Select(t => new ClaudeTaskResponseDto(
            t.Id, t.Title, t.Scope, t.Status.ToString().ToLower(),
            JsonSerializer.Deserialize<string[]>(t.AssignedAgents) ?? [], t.SkillUsed,
            t.StartedAt, t.CompletedAt, t.CreatedAt)).ToList();
    }
}
