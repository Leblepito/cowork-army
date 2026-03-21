using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CoworkArmy.Domain.DataBridge;

namespace CoworkArmy.Infrastructure.DataBridge;

/// <summary>HTTP client for u2algo.com /api/cowork/* endpoints.</summary>
public class U2AlgoClient
{
    private readonly HttpClient _http;
    private readonly ILogger<U2AlgoClient> _log;
    private readonly int _timeoutMs;

    public U2AlgoClient(HttpClient http, IConfiguration config, ILogger<U2AlgoClient> log)
    {
        _http = http;
        _log = log;
        _timeoutMs = config.GetValue("DataBridge:TimeoutMs", 5000);
    }

    public async Task<TradeFeed> FetchAsync(CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);

            // Fetch all 3 endpoints in parallel
            var pricesTask = FetchJsonAsync("/api/cowork/prices", cts.Token);
            var positionsTask = FetchJsonAsync("/api/cowork/positions", cts.Token);
            var signalsTask = FetchJsonAsync("/api/cowork/signals", cts.Token);

            await Task.WhenAll(pricesTask, positionsTask, signalsTask);

            var prices = pricesTask.Result;
            var positions = positionsTask.Result;
            var signals = signalsTask.Result;

            return new TradeFeed(
                BtcPrice: GetDecimal(prices, "btc", "price"),
                EthPrice: GetDecimal(prices, "eth", "price"),
                BtcChange24h: GetDecimal(prices, "btc", "change24h"),
                EthChange24h: GetDecimal(prices, "eth", "change24h"),
                OpenPositions: GetInt(positions, "open"),
                TotalPnl: GetDecimal(positions, "totalPnl"),
                ActiveSignals: GetInt(signals, "active"),
                FetchedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "U2Algo fetch failed, returning fallback");
            return SimulatedTradeFeed();
        }
    }

    private async Task<JsonElement> FetchJsonAsync(string path, CancellationToken ct)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonDocument.Parse(json).RootElement;
    }

    private static decimal GetDecimal(JsonElement root, string key, string? nested = null)
    {
        if (!root.TryGetProperty(key, out var el)) return 0;
        if (nested is not null)
        {
            if (!el.TryGetProperty(nested, out var inner)) return 0;
            el = inner;
        }
        return el.TryGetDecimal(out var val) ? val : 0;
    }

    private static int GetInt(JsonElement root, string key)
        => root.TryGetProperty(key, out var el) && el.TryGetInt32(out var val) ? val : 0;

    private static TradeFeed SimulatedTradeFeed()
    {
        var rng = Random.Shared;
        return new TradeFeed(
            BtcPrice: 64000 + rng.Next(-2000, 2000),
            EthPrice: 3100 + rng.Next(-200, 200),
            BtcChange24h: Math.Round((decimal)(rng.NextDouble() * 6 - 3), 2),
            EthChange24h: Math.Round((decimal)(rng.NextDouble() * 6 - 3), 2),
            OpenPositions: rng.Next(0, 8),
            TotalPnl: Math.Round((decimal)(rng.NextDouble() * 5000 - 1000), 2),
            ActiveSignals: rng.Next(0, 6),
            FetchedAt: DateTime.UtcNow);
    }
}
