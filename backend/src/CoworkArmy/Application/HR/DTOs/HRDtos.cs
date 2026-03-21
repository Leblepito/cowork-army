namespace CoworkArmy.Application.HR.DTOs;

public record SpawnRequestDto(string Reason, string Department);
public record RetireRequestDto(string Reason);
public record WarnRequestDto(string Reason);
public record SpawnResultDto(string AgentId, string Name, string Icon, string Department, string DesignedBy);
public record ProposalDto(string Id, string Type, string? AgentId, string Reason, string Status, DateTime CreatedAt);
public record PerformanceDto(string AgentId, int TasksCompleted, int TasksFailed, double AvgResponseMs, long TotalTokens, double EstimatedCost, int Warnings, string Grade, DateTime? LastActiveAt);
