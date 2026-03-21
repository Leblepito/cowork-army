namespace CoworkArmy.Application.Interfaces;

public interface ILlmProvider
{
    string Name { get; }
    Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct = default);
    double EstimateCost(int inputTokens, int outputTokens, string model);
}

public interface ILlmProviderFactory
{
    ILlmProvider GetByName(string name);
    ILlmProvider GetByTier(string tier);
}

public record LlmMessage(string Role, string Content);

public record LlmRequest(
    string Model,
    string SystemPrompt,
    List<LlmMessage> Messages,
    int MaxTokens = 2048);

public record LlmResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    double CostUsd,
    string Model);
