using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.LLM;

public class OpenAIProvider : ILlmProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenAIProvider> _log;

    public string Name => "openai";

    private static readonly Dictionary<string, (double Input, double Output)> ModelCosts = new()
    {
        ["gpt-4o"] = (2.50, 10.00),
        ["gpt-4o-mini"] = (0.15, 0.60),
        ["gpt-4-turbo"] = (10.00, 30.00),
    };

    public OpenAIProvider(IConfiguration config, ILogger<OpenAIProvider> log)
    {
        _log = log;
        _http = new HttpClient { BaseAddress = new Uri("https://api.openai.com/") };
        var apiKey = config["OPENAI_API_KEY"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct = default)
    {
        var model = request.Model.StartsWith("gpt") ? request.Model : "gpt-4o-mini";

        var messages = new List<object>();

        if (!string.IsNullOrEmpty(request.SystemPrompt))
            messages.Add(new { role = "system", content = request.SystemPrompt });

        foreach (var msg in request.Messages)
            messages.Add(new { role = msg.Role, content = msg.Content });

        var body = new
        {
            model,
            messages,
            max_tokens = request.MaxTokens
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _log.LogInformation("OpenAI request: model={Model}, messages={Count}", model, request.Messages.Count);

        var response = await _http.PostAsync("v1/chat/completions", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("OpenAI API error {Status}: {Body}", response.StatusCode, responseBody);
            throw new Exception($"OpenAI API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);
        if (result == null) throw new Exception("Failed to parse OpenAI response");

        var outputText = result.Choices?.FirstOrDefault()?.Message?.Content ?? "";
        var inputTokens = result.Usage?.PromptTokens ?? 0;
        var outputTokens = result.Usage?.CompletionTokens ?? 0;
        var cost = EstimateCost(inputTokens, outputTokens, model);

        _log.LogInformation("OpenAI response: tokens={In}+{Out}, cost=${Cost:F4}", inputTokens, outputTokens, cost);

        return new LlmResponse(outputText, inputTokens, outputTokens, cost, model);
    }

    public double EstimateCost(int inputTokens, int outputTokens, string model)
    {
        if (!ModelCosts.TryGetValue(model, out var costs)) costs = ModelCosts["gpt-4o-mini"];
        return (inputTokens * costs.Input / 1_000_000.0) + (outputTokens * costs.Output / 1_000_000.0);
    }

    private record OpenAIResponse
    {
        [JsonPropertyName("choices")] public List<OpenAIChoice>? Choices { get; init; }
        [JsonPropertyName("usage")] public OpenAIUsage? Usage { get; init; }
    }
    private record OpenAIChoice
    {
        [JsonPropertyName("message")] public OpenAIMessage? Message { get; init; }
    }
    private record OpenAIMessage
    {
        [JsonPropertyName("content")] public string? Content { get; init; }
    }
    private record OpenAIUsage
    {
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; init; }
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; init; }
    }
}
