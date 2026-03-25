namespace CoworkArmy.Domain.ClaudeBridge;

public interface IClaudeBridgeRepository
{
    Task<ClaudeEvent> RecordEventAsync(ClaudeEvent ev);
    Task<ClaudeTask> CreateTaskAsync(ClaudeTask task);
    Task<ClaudeTask?> GetTaskAsync(string id);
    Task SaveChangesAsync();
    Task<List<ClaudeTask>> GetActiveTasksAsync();
    Task<List<ClaudeEvent>> GetRecentEventsAsync(int limit = 50);
}
