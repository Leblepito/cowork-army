namespace CoworkArmy.Domain.Agents;

public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = "";
    public string? UserId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
