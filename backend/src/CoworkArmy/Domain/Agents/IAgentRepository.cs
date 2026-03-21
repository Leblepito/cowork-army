namespace CoworkArmy.Domain.Agents;
public interface IAgentRepository { Task<Agent?> GetByIdAsync(string id); Task<List<Agent>> GetAllAsync(); Task<List<Agent>> GetByDepartmentAsync(string dept); Task AddAsync(Agent agent); Task UpdateAsync(Agent agent); Task DeleteAsync(string id); Task<bool> ExistsAsync(string id); }
