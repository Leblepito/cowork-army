namespace CoworkArmy.Domain.Agents;
public record AgentStatus(string AgentId, string Status = "idle", bool Alive = false, List<string>? Lines = null) { public List<string> Lines { get; set; } = Lines ?? new(); public DateTime? StartedAt { get; set; } }
