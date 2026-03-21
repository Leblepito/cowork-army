namespace CoworkArmy.Domain.Common;
public abstract class Entity { public string Id { get; protected set; } = ""; public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow; }
public abstract class AggregateRoot : Entity { }
public class DomainException : Exception { public DomainException(string msg) : base(msg) { } }
