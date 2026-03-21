using CoworkArmy.Domain.Tasks;
using Xunit;
using TaskStatus = CoworkArmy.Domain.Tasks.TaskStatus;

namespace CoworkArmy.Tests;

public class TaskStateMachineTests
{
    [Fact]
    public void New_task_is_Pending()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Start_sets_Running()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        task.Start();
        Assert.Equal(TaskStatus.Running, task.Status);
    }

    [Fact]
    public void Complete_sets_Succeeded_with_timestamp()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        task.Start();
        task.Complete();
        Assert.Equal(TaskStatus.Succeeded, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void Fail_sets_Failed_and_stores_reason()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        task.Start();
        task.Fail("timeout");
        Assert.Equal(TaskStatus.Failed, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void TimeOut_sets_TimedOut()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        task.Start();
        task.TimeOut();
        Assert.Equal(TaskStatus.TimedOut, task.Status);
    }

    [Fact]
    public void Cancel_sets_Cancelled()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        task.Cancel();
        Assert.Equal(TaskStatus.Cancelled, task.Status);
    }

    [Fact]
    public void Task_id_has_correct_prefix()
    {
        var task = AgentTask.Create("test", "agent1", "ceo");
        Assert.StartsWith("task-", task.Id);
    }
}
