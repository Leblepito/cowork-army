using CoworkArmy.Domain.ClaudeBridge;
using CoworkArmy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoworkArmy.Infrastructure.ClaudeBridge;

public class ClaudeBridgeRepository : IClaudeBridgeRepository
{
    private readonly CoworkDbContext _db;
    public ClaudeBridgeRepository(CoworkDbContext db) => _db = db;

    public async Task<ClaudeEvent> RecordEventAsync(ClaudeEvent ev)
    {
        _db.ClaudeEvents.Add(ev);
        await _db.SaveChangesAsync();
        return ev;
    }

    public async Task<ClaudeTask> CreateTaskAsync(ClaudeTask task)
    {
        _db.ClaudeTasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<ClaudeTask?> GetTaskAsync(string id)
        => await _db.ClaudeTasks.FindAsync(id);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();

    public async Task<List<ClaudeTask>> GetActiveTasksAsync()
        => await _db.ClaudeTasks
            .Where(t => t.Status == ClaudeTaskStatus.Pending || t.Status == ClaudeTaskStatus.Running)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<ClaudeEvent>> GetRecentEventsAsync(int limit = 50)
        => await _db.ClaudeEvents
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync();
}
