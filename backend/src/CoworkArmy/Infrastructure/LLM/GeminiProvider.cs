using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.LLM;

public class GeminiProvider : ILlmProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiProvider> _log;
    private readonly string _apiKey;

    public string Name => "gemini";

    private static readonly Dictionary<string, (double Input, double Output)> ModelCosts = new()
    {
        ["gemini-2.0-flash"] = (0.075, 0.30),
        ["gemini-1.5-pro"] = (1.25, 5.00),
        ["gemini-1.5-flash"] = (0.075, 0.30),
    };

    public GeminiProvider(IConfiguration config, ILogger<GeminiProvider> log)
    {
        _log = log;
        _http = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
        _apiKey = config["GEMINI_API_KEY"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
    }

    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct = default)
    {
        var model = request.Model.StartsWith("gemini") ? request.Model : "gemini-2.0-flash";

        // Build contents array from messages
        var contents = new List<object>();

        // Add system prompt as first user message if present
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            contents.Add(new { role = "user", parts = new[] { new { text = request.SystemPrompt } } });
            contents.Add(new { role = "model", parts = new[] { new { text = "Understood. I'll follow these instructions." } } });
        }

        foreach (var msg in request.Messages)
        {
            var role = msg.Role == "assistant" ? "model" : "user";
            contents.Add(new { role, parts = new[] { new { text = msg.Content } } });
        }

        var body = new
        {
            contents,
            generationConfig = new { maxOutputTokens = request.MaxTokens }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"v1beta/models/{model}:generateContent?key={_apiKey}";
        _log.LogInformation("Gemini request: model={Model}, messages={Count}", model, request.Messages.Count);

        var response = await _http.PostAsync(url, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("Gemini API error {Status}: {Body}", response.StatusCode, responseBody);
            throw new Exception($"Gemini API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<GeminiResponse>(responseBody);
        if (result == null) throw new Exception("Failed to parse Gemini response");

        var outputText = result.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "";
        var inputTokens = result.UsageMetadata?.PromptTokenCount ?? 0;
        var outputTokens = result.UsageMetadata?.CandidatesTokenCount ?? 0;
        var cost = EstimateCost(inputTokens, outputTokens, model);

        _log.LogInformation("Gemini response: tokens={In}+{Out}, cost=${Cost:F4}", inputTokens, outputTokens, cost);

        return new LlmResponse(outputText, inputTokens, outputTokens, cost, model);
    }

    public double EstimateCost(int inputTokens, int outputTokens, string model)
    {
        if (!ModelCosts.TryGetValue(model, out var costs)) costs = ModelCosts["gemini-2.0-flash"];
        return (inputTokens * costs.Input / 1_000_000.0) + (outputTokens * costs.Output / 1_000_000.0);
    }

    private record GeminiResponse
    {
        [JsonPropertyName("candidates")] public List<GeminiCandidate>? Candidates { get; init; }
        [JsonPropertyName("usageMetadata")] public GeminiUsage? UsageMetadata { get; init; }
    }
    private record GeminiCandidate
    {
        [JsonPropertyName("content")] public GeminiContent? Content { get; init; }
    }
    private record GeminiContent
    {
        [JsonPropertyName("parts")] public List<GeminiPart>? Parts { get; init; }
    }
    private record GeminiPart
    {
        [JsonPropertyName("text")] public string? Text { get; init; }
    }
    private record GeminiUsage
    {
        [JsonPropertyName("promptTokenCount")] public int PromptTokenCount { get; init; }
        [JsonPropertyName("candidatesTokenCount")] public int CandidatesTokenCount { get; init; }
    }
}
