namespace CoworkArmy.Domain.Tasks;
public interface ITaskRepository { Task<AgentTask?> GetByIdAsync(string id); Task<List<AgentTask>> GetRecentAsync(int limit = 50, int offset = 0); Task AddAsync(AgentTask task); Task UpdateAsync(AgentTask task); }
