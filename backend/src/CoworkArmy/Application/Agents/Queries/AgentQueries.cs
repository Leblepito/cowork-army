using CoworkArmy.Application.Agents.DTOs;
using CoworkArmy.Domain.Agents;

namespace CoworkArmy.Application.Agents.Queries;

public class GetAgentsQueryHandler
{
    private readonly IAgentRepository _repo;
    public GetAgentsQueryHandler(IAgentRepository repo) => _repo = repo;

    public async Task<List<AgentResponseDto>> HandleAsync()
    {
        var agents = await _repo.GetAllAsync();
        return agents.Select(a => new AgentResponseDto(
            a.Id, a.Name, a.Icon, a.Tier.ToString(),
            a.Color, a.Department, a.Description,
            a.Skills, a.IsBase, a.CreatedAt)).ToList();
    }

    public async Task<AgentResponseDto?> HandleByIdAsync(string id)
    {
        var a = await _repo.GetByIdAsync(id);
        if (a == null) return null;
        return new AgentResponseDto(
            a.Id, a.Name, a.Icon, a.Tier.ToString(),
            a.Color, a.Department, a.Description,
            a.Skills, a.IsBase, a.CreatedAt);
    }
}
