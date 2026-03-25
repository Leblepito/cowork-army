using CoworkArmy.Application.ClaudeBridge.DTOs;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.ClaudeBridge;

namespace CoworkArmy.Application.ClaudeBridge;

public class CompleteClaudeTaskHandler
{
    private readonly IClaudeBridgeRepository _repo;
    private readonly IRealtimeNotifier _notifier;

    public CompleteClaudeTaskHandler(IClaudeBridgeRepository repo, IRealtimeNotifier notifier)
    { _repo = repo; _notifier = notifier; }

    public async Task<ClaudeTask?> HandleAsync(string taskId, CompleteClaudeTaskDto dto)
    {
        var task = await _repo.GetTaskAsync(taskId);
        if (task is null) return null;
        var durationMs = task.StartedAt.HasValue ? (int)(DateTime.UtcNow - task.StartedAt.Value).TotalMilliseconds : 0;
        switch (dto.Status) { case "failed": task.Fail(); break; case "timed_out": task.Timeout(); break; default: task.Complete(); break; }
        await _repo.SaveChangesAsync();
        await _notifier.SendClaudeTaskCompleteAsync(task.Id, task.Status.ToString().ToLower(), durationMs);
        return task;
    }
}
