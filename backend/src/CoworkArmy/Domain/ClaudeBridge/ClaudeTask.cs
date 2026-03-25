using System.Text.Json;
using CoworkArmy.Domain.Common;

namespace CoworkArmy.Domain.ClaudeBridge;

public class ClaudeTask : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public ClaudeTaskStatus Status { get; private set; }
    public string AssignedAgents { get; private set; } = "[]";
    public string? SkillUsed { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private ClaudeTask() { }

    public static ClaudeTask Create(string title, string scope, string[] agents, string? skill = null)
    {
        return new ClaudeTask
        {
            Id = $"ct-{Guid.NewGuid():N}",
            Title = title, Scope = scope,
            Status = ClaudeTaskStatus.Pending,
            AssignedAgents = JsonSerializer.Serialize(agents),
            SkillUsed = skill, CreatedAt = DateTime.UtcNow
        };
    }

    public void Start() { Status = ClaudeTaskStatus.Running; StartedAt = DateTime.UtcNow; }
    public void Complete() { Status = ClaudeTaskStatus.Succeeded; CompletedAt = DateTime.UtcNow; }
    public void Fail() { Status = ClaudeTaskStatus.Failed; CompletedAt = DateTime.UtcNow; }
    public void Timeout() { Status = ClaudeTaskStatus.TimedOut; CompletedAt = DateTime.UtcNow; }
}
