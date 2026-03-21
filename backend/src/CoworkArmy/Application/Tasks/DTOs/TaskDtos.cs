using CoworkArmy.Domain.Tasks;

namespace CoworkArmy.Application.Tasks.DTOs;

public record TaskCreateDto(string Title, string Description = "", string AssignedTo = "", string Priority = "normal");
public record TaskResponseDto(string Id, string Title, string Description, string AssignedTo, string CreatedBy, string Priority, string Status, DateTime CreatedAt, DateTimeOffset? CompletedAt);
public record DelegateDto(string Title, string Description = "", string Priority = "normal");
