namespace CoworkArmy.Domain.Agents;

public interface IAgentStateRepository
{
    Task<AgentState?> GetByAgentIdAsync(string agentId);
    Task AddAsync(AgentState state);
    Task UpdateAsync(AgentState state);
}
