namespace CoworkArmy.Domain.Events;
public interface IEventRepository { Task AddAsync(AgentEvent evt); Task<List<AgentEvent>> GetRecentAsync(int limit = 50, int offset = 0); }
