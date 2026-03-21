namespace CoworkArmy.Application.Chat.DTOs;

public record SendMessageDto(string AgentId, string Message, string? ConversationId = null);
public record ChatMessageResponseDto(string Id, string Role, string Content, DateTime Timestamp);
public record ConversationDto(string Id, string AgentId, string Title, List<ChatMessageResponseDto> Messages, DateTime UpdatedAt);
public record ChatResponseDto(string ConversationId, ChatMessageResponseDto UserMessage, ChatMessageResponseDto AssistantMessage, int Tokens, double Cost);
