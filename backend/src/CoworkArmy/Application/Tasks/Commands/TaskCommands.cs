using CoworkArmy.Application.Interfaces;
using CoworkArmy.Application.Tasks.DTOs;
using CoworkArmy.Domain.Tasks;

namespace CoworkArmy.Application.Tasks.Commands;

public record CreateTaskCommand(TaskCreateDto Data);

public class CreateTaskCommandHandler
{
    private readonly ITaskRepository _repo;
    private readonly IRealtimeNotifier _notifier;
    private readonly ITaskRouter _router;

    public CreateTaskCommandHandler(ITaskRepository repo, IRealtimeNotifier notifier, ITaskRouter router)
    { _repo = repo; _notifier = notifier; _router = router; }

    public async Task<TaskResponseDto> HandleAsync(CreateTaskCommand cmd)
    {
        var d = cmd.Data;
        var assignee = string.IsNullOrEmpty(d.AssignedTo)
            ? _router.Route(d.Title + " " + d.Description)
            : d.AssignedTo;
        var priority = Enum.TryParse<TaskPriority>(d.Priority, true, out var p) ? p : TaskPriority.Normal;

        var task = AgentTask.Create(d.Title, assignee, "ceo", priority, d.Description);
        await _repo.AddAsync(task);
        await _notifier.SendEventAsync("task_created", assignee, $"New task: {d.Title}");

        return new TaskResponseDto(
            task.Id, task.Title, task.Description, task.AssignedTo,
            task.CreatedBy, task.Priority.ToString(), task.Status.ToString(),
            task.CreatedAt, task.CompletedAt);
    }
}
