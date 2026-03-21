namespace CoworkArmy.Domain;

public interface IPerformanceRepository
{
    Task<AgentPerformance?> GetByAgentIdAsync(string agentId);
    Task<List<AgentPerformance>> GetAllAsync();
    Task AddAsync(AgentPerformance perf);
    Task UpdateAsync(AgentPerformance perf);
}
