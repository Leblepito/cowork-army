namespace CoworkArmy.Application.ClaudeBridge.DTOs;

public record RecordClaudeEventDto(string Tool, string AgentId, string Summary, string? FilePath = null, string? TaskId = null, string? Metadata = null);
public record StartClaudeTaskDto(string Title, string Scope, string[] Agents, string? Skill = null);
public record CompleteClaudeTaskDto(string Status);
public record ClaudeEventResponseDto(int Id, string Tool, string? FilePath, string AgentId, string? TaskId, string Summary, DateTime CreatedAt);
public record ClaudeTaskResponseDto(string Id, string Title, string Scope, string Status, string[] AssignedAgents, string? SkillUsed, DateTime? StartedAt, DateTime? CompletedAt, DateTime CreatedAt);
