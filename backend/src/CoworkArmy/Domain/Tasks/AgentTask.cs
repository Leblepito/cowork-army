using CoworkArmy.Domain.Common;
namespace CoworkArmy.Domain.Tasks;
public class AgentTask : AggregateRoot
{
    public string Title { get; private set; } = ""; public string Description { get; private set; } = "";
    public string AssignedTo { get; private set; } = ""; public string CreatedBy { get; private set; } = "ceo";
    public TaskPriority Priority { get; private set; } = TaskPriority.Normal; public TaskStatus Status { get; private set; } = TaskStatus.Pending;
    public string Log { get; private set; } = "[]"; public DateTimeOffset? CompletedAt { get; private set; }
    private AgentTask() { }
    public static AgentTask Create(string title, string assignedTo, string createdBy, TaskPriority priority = TaskPriority.Normal, string desc = "")
        => new() { Id = $"task-{Guid.NewGuid().ToString()[..8]}", Title = title, Description = desc, AssignedTo = assignedTo, CreatedBy = createdBy, Priority = priority };
    public void Start() { Status = TaskStatus.Running; }
    public void Complete() { Succeed(); }
    public void Fail(string reason) { Status = TaskStatus.Failed; CompletedAt = DateTimeOffset.UtcNow; Log = reason; }
    public void Succeed() { Status = TaskStatus.Succeeded; CompletedAt = DateTimeOffset.UtcNow; }
    public void TimeOut() { Status = TaskStatus.TimedOut; CompletedAt = DateTimeOffset.UtcNow; }
    public void Cancel() { Status = TaskStatus.Cancelled; CompletedAt = DateTimeOffset.UtcNow; }
}
