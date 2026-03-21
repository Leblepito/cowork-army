using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.LLM;

public class AnthropicProvider : ILlmProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<AnthropicProvider> _log;
    private readonly Dictionary<string, (double Input, double Output)> _modelCosts;

    public string Name => "anthropic";

    public AnthropicProvider(IConfiguration config, ILogger<AnthropicProvider> log)
    {
        _log = log;
        _http = new HttpClient { BaseAddress = new Uri("https://api.anthropic.com/") };
        var apiKey = config["ANTHROPIC_API_KEY"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
        _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _modelCosts = new()
        {
            ["claude-sonnet-4-6"] = (config.GetValue("LLM:Costs:Sonnet:Input", 3.00), config.GetValue("LLM:Costs:Sonnet:Output", 15.00)),
            ["claude-haiku-4-5"] = (config.GetValue("LLM:Costs:Haiku:Input", 0.80), config.GetValue("LLM:Costs:Haiku:Output", 4.00)),
            ["claude-opus-4-6"] = (config.GetValue("LLM:Costs:Opus:Input", 15.00), config.GetValue("LLM:Costs:Opus:Output", 75.00)),
        };
    }

    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct = default)
    {
        var body = new
        {
            model = request.Model,
            max_tokens = request.MaxTokens,
            system = request.SystemPrompt,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _log.LogInformation("LLM request: model={Model}, messages={Count}", request.Model, request.Messages.Count);

        var response = await _http.PostAsync("v1/messages", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("Anthropic API error {Status}: {Body}", response.StatusCode, responseBody);
            throw new Exception($"Anthropic API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);
        if (result == null) throw new Exception("Failed to parse Anthropic response");

        var outputText = result.Content?.FirstOrDefault()?.Text ?? "";
        var inputTokens = result.Usage?.InputTokens ?? 0;
        var outputTokens = result.Usage?.OutputTokens ?? 0;
        var cost = EstimateCost(inputTokens, outputTokens, request.Model);

        _log.LogInformation("LLM response: tokens={In}+{Out}, cost=${Cost:F4}", inputTokens, outputTokens, cost);

        return new LlmResponse(outputText, inputTokens, outputTokens, cost, request.Model);
    }

    public double EstimateCost(int inputTokens, int outputTokens, string model)
    {
        if (!_modelCosts.TryGetValue(model, out var costs)) costs = _modelCosts["claude-haiku-4-5"];
        return (inputTokens * costs.Input / 1_000_000.0) + (outputTokens * costs.Output / 1_000_000.0);
    }

    private record AnthropicResponse
    {
        [JsonPropertyName("content")] public List<ContentBlock>? Content { get; init; }
        [JsonPropertyName("usage")] public UsageBlock? Usage { get; init; }
    }
    private record ContentBlock { [JsonPropertyName("text")] public string? Text { get; init; } }
    private record UsageBlock
    {
        [JsonPropertyName("input_tokens")] public int InputTokens { get; init; }
        [JsonPropertyName("output_tokens")] public int OutputTokens { get; init; }
    }
}
