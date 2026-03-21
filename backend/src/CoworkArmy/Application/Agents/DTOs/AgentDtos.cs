namespace CoworkArmy.Application.Agents.DTOs;

public record AgentResponseDto(
    string Id, string Name, string Icon, string Tier,
    string Color, string Department, string Description,
    string Skills, bool IsBase, DateTime CreatedAt);

public record AgentCreateDto(
    string Name, string Icon, string Tier, string Color,
    string Department, string Description,
    string Skills = "[]", string SystemPrompt = "");

public record AgentStatusDto(
    string AgentId, string Status, bool Alive,
    List<string> Lines, DateTime? StartedAt);
