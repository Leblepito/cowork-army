using CoworkArmy.Application.ClaudeBridge.DTOs;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.ClaudeBridge;

namespace CoworkArmy.Application.ClaudeBridge;

public class RecordClaudeEventHandler
{
    private readonly IClaudeBridgeRepository _repo;
    private readonly IRealtimeNotifier _notifier;

    public RecordClaudeEventHandler(IClaudeBridgeRepository repo, IRealtimeNotifier notifier)
    { _repo = repo; _notifier = notifier; }

    public async Task<ClaudeEvent> HandleAsync(RecordClaudeEventDto dto)
    {
        var ev = ClaudeEvent.Create(dto.Tool, dto.AgentId, dto.Summary, dto.FilePath, dto.TaskId, dto.Metadata ?? "{}");
        await _repo.RecordEventAsync(ev);
        await _notifier.SendClaudeActionAsync(dto.Tool, dto.FilePath, dto.AgentId, dto.Summary);
        var status = dto.Tool switch { "Edit" or "Write" => "coding", "Bash" => "working", "Grep" or "Glob" => "thinking", _ => "working" };
        await _notifier.SendStatusChangeAsync(dto.AgentId, status);
        return ev;
    }
}
