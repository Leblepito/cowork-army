using CoworkArmy.Application.ClaudeBridge.DTOs;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.ClaudeBridge;

namespace CoworkArmy.Application.ClaudeBridge;

public class StartClaudeTaskHandler
{
    private readonly IClaudeBridgeRepository _repo;
    private readonly IRealtimeNotifier _notifier;

    public StartClaudeTaskHandler(IClaudeBridgeRepository repo, IRealtimeNotifier notifier)
    { _repo = repo; _notifier = notifier; }

    public async Task<ClaudeTask> HandleAsync(StartClaudeTaskDto dto)
    {
        var task = ClaudeTask.Create(dto.Title, dto.Scope, dto.Agents, dto.Skill);
        task.Start();
        await _repo.CreateTaskAsync(task);
        await _notifier.SendClaudeTaskStartAsync(task.Id, task.Title, task.Scope, dto.Agents);
        return task;
    }
}
