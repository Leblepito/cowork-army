namespace CoworkArmy.Domain.Events;
public class AgentEvent { public int Id { get; set; } public string Type { get; set; } = "info"; public string AgentId { get; set; } = ""; public string Message { get; set; } = ""; public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow; }
