using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Commands;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.Tasks;

namespace CoworkArmy.Application.CommandChain;

public class RunCommandChainHandler
{
    private readonly IAgentRepository _agents;
    private readonly ITaskRepository _tasks;
    private readonly IEventRepository _events;
    private readonly IRealtimeNotifier _notifier;
    private readonly IStatusTracker _tracker;

    public RunCommandChainHandler(
        IAgentRepository agents, ITaskRepository tasks, IEventRepository events,
        IRealtimeNotifier notifier, IStatusTracker tracker)
    {
        _agents = agents; _tasks = tasks; _events = events;
        _notifier = notifier; _tracker = tracker;
    }

    public async Task ExecuteAsync(CeoOrder order)
    {
        // Verify all agents are idle
        var allIds = new List<string> { "ceo", order.DirectorId };
        allIds.AddRange(order.Tasks.Select(t => t.WorkerId));
        if (allIds.Any(id => _tracker.Get(id).Status != "idle")) return;

        var dir = await _agents.GetByIdAsync(order.DirectorId);
        if (dir == null) return;

        // ═══ PHASE 1: CEO → Director ═══
        _tracker.Set("ceo", "commanding", $"📢 {order.CeoMessage}");
        await _notifier.SendStatusChangeAsync("ceo", "commanding");
        await _notifier.SendCommandAsync("ceo_to_dir", "ceo", order.DirectorId, order.CeoMessage);
        await _notifier.SendMovementAsync("ceo", order.DirectorId, 2000);
        await _notifier.SendEventAsync("command", "ceo", $"👑 CEO: \"{order.CeoMessage}\"");
        await _events.AddAsync(new AgentEvent { Type = "command", AgentId = "ceo", Message = $"CEO → {dir.Icon} {dir.Name}: {order.CeoMessage}" });

        _tracker.Set(order.DirectorId, "talking", $"👑 CEO talimatı: {order.CeoMessage}");
        await _notifier.SendStatusChangeAsync(order.DirectorId, "talking");

        await Task.Delay(3000);

        // Director acknowledges
        await _notifier.SendConversationAsync(order.DirectorId, dir.Icon, "ceo", "Anlaşıldı, organize ediyorum");
        _tracker.AddLog(order.DirectorId, "✅ CEO talimatı kabul edildi");
        await _events.AddAsync(new AgentEvent { Type = "response", AgentId = order.DirectorId, Message = $"{dir.Icon}: Anlaşıldı" });

        await Task.Delay(2000);
        _tracker.Set("ceo", "idle");
        await _notifier.SendStatusChangeAsync("ceo", "idle");
        await _notifier.SendMovementAsync("ceo", "ceo", 2000);

        // ═══ PHASE 2: Director → Workers ═══
        await Task.Delay(1500);
        _tracker.Set(order.DirectorId, "commanding", $"📢 {order.DirectorMessage}");
        await _notifier.SendStatusChangeAsync(order.DirectorId, "commanding");
        await _notifier.SendEventAsync("delegate", order.DirectorId, $"{dir.Icon} ekibe: \"{order.DirectorMessage}\"");
        await _events.AddAsync(new AgentEvent { Type = "delegate", AgentId = order.DirectorId, Message = $"{dir.Icon} → ekip: {order.DirectorMessage}" });

        foreach (var (workerId, taskMsg) in order.Tasks)
        {
            var worker = await _agents.GetByIdAsync(workerId);
            if (worker == null) continue;

            await Task.Delay(2500);

            // Director delegates
            await _notifier.SendCommandAsync("dir_to_worker", order.DirectorId, workerId, taskMsg);
            await _notifier.SendMovementAsync(order.DirectorId, workerId, 2000);
            await _notifier.SendDocumentTransferAsync(order.DirectorId, workerId, "task");
            await _notifier.SendConversationAsync(order.DirectorId, dir.Icon, workerId, $"→ {worker.Icon} {taskMsg}");
            await _events.AddAsync(new AgentEvent { Type = "task_assign", AgentId = order.DirectorId, Message = $"{dir.Icon}→{worker.Icon}: {taskMsg}" });

            await Task.Delay(1200);

            // Worker starts
            _tracker.Set(workerId, "working", $"▶ {taskMsg}");
            await _notifier.SendStatusChangeAsync(workerId, "working");
            await _notifier.SendConversationAsync(workerId, worker.Icon, order.DirectorId, "Tamam, başlıyorum!");
            await _events.AddAsync(new AgentEvent { Type = "work_start", AgentId = workerId, Message = $"{worker.Icon}: Başlıyorum — {taskMsg}" });

            // Create persisted task
            var task = AgentTask.Create(taskMsg, workerId, order.DirectorId, Domain.Tasks.TaskPriority.High, $"CEO → {dir.Name} → {worker.Name}");
            task.Start();
            await _tasks.AddAsync(task);

            // Worker finishes (fire-and-forget)
            var wId = workerId;
            var wIcon = worker.Icon;
            var tId = task.Id;
            _ = Task.Run(async () =>
            {
                await Task.Delay(4000 + Random.Shared.Next(3000));
                _tracker.Set(wId, "idle", $"✅ Tamamlandı: {taskMsg}");
                await _notifier.SendStatusChangeAsync(wId, "idle");
                await _notifier.SendTaskEffectAsync(wId, "complete");
                await _notifier.SendEventAsync("complete", wId, $"{wIcon} tamamladı: {taskMsg[..Math.Min(25, taskMsg.Length)]}");
            });
        }

        // Director goes idle
        await Task.Delay(2000);
        _tracker.Set(order.DirectorId, "idle");
        await _notifier.SendStatusChangeAsync(order.DirectorId, "idle");
        await _notifier.SendMovementAsync(order.DirectorId, order.DirectorId, 2000);
    }
}
