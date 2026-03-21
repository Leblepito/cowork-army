namespace CoworkArmy.Domain.Commands;
public record CeoOrder(string CeoMessage, string Department, string DirectorId, string DirectorMessage, (string WorkerId, string TaskMessage)[] Tasks);
