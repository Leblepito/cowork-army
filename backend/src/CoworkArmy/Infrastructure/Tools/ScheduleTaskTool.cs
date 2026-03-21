using CoworkArmy.Domain.Tasks;
using CoworkArmy.Domain.Tools;

namespace CoworkArmy.Infrastructure.Tools;

public class ScheduleTaskTool : ITool
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleTaskTool> _log;

    public string Name => "schedule_task";
    public string Description => "Create and schedule a new task for an agent. Params: title, assigned_to, created_by (optional), priority (optional: Low|Normal|High|Critical), description (optional)";
    public ToolPermission Permission => ToolPermission.Safe;
    public string[] RequiredParams => new[] { "title", "assigned_to" };

    public ScheduleTaskTool(IServiceScopeFactory scopeFactory, ILogger<ScheduleTaskTool> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    public async Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
            return new ToolResult(false, "", Error: "Missing 'title' parameter");

        if (!parameters.TryGetValue("assigned_to", out var assignedTo) || string.IsNullOrWhiteSpace(assignedTo))
            return new ToolResult(false, "", Error: "Missing 'assigned_to' parameter");

        var createdBy = parameters.GetValueOrDefault("created_by", "system");
        var description = parameters.GetValueOrDefault("description", "");
        var priorityStr = parameters.GetValueOrDefault("priority", "Normal");

        var priority = Enum.TryParse<TaskPriority>(priorityStr, true, out var p) ? p : TaskPriority.Normal;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

            var task = AgentTask.Create(title, assignedTo, createdBy, priority, description);
            await repo.AddAsync(task);

            _log.LogInformation("Task scheduled: id={Id}, title={Title}, assignedTo={Agent}",
                task.Id, title, assignedTo);

            return new ToolResult(true, $"Task created: id={task.Id}, title='{title}', assigned_to={assignedTo}, priority={priority}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ScheduleTask failed: {Title}", title);
            return new ToolResult(false, "", Error: $"Failed to create task: {ex.Message}");
        }
    }
}
