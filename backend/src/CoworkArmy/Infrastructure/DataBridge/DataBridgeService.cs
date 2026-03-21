using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CoworkArmy.Domain.DataBridge;

namespace CoworkArmy.Infrastructure.DataBridge;

/// <summary>IDataBridgeService implementation with TTL caching and fallback.</summary>
public class DataBridgeService : IDataBridgeService
{
    private readonly U2AlgoClient _u2algo;
    private readonly LebLepitoClient _leblepito;
    private readonly ILogger<DataBridgeService> _log;
    private readonly bool _fallbackToSim;

    // 30-second TTL cache
    private TradeFeed? _tradeFeedCache;
    private MedicalFeed? _medicalFeedCache;
    private HotelFeed? _hotelFeedCache;
    private DateTime _tradeExpiry = DateTime.MinValue;
    private DateTime _medicalExpiry = DateTime.MinValue;
    private DateTime _hotelExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private readonly object _lock = new();

    public DataBridgeService(
        U2AlgoClient u2algo,
        LebLepitoClient leblepito,
        IConfiguration config,
        ILogger<DataBridgeService> log)
    {
        _u2algo = u2algo;
        _leblepito = leblepito;
        _log = log;
        _fallbackToSim = config.GetValue("DataBridge:FallbackToSimulation", true);
    }

    public async Task<TradeFeed> GetTradeFeedAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_tradeFeedCache is not null && DateTime.UtcNow < _tradeExpiry)
                return _tradeFeedCache;
        }

        var feed = await _u2algo.FetchAsync(ct);
        lock (_lock)
        {
            _tradeFeedCache = feed;
            _tradeExpiry = DateTime.UtcNow.Add(CacheTtl);
        }
        return feed;
    }

    public async Task<MedicalFeed> GetMedicalFeedAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_medicalFeedCache is not null && DateTime.UtcNow < _medicalExpiry)
                return _medicalFeedCache;
        }

        var feed = await _leblepito.FetchMedicalAsync(ct);
        lock (_lock)
        {
            _medicalFeedCache = feed;
            _medicalExpiry = DateTime.UtcNow.Add(CacheTtl);
        }
        return feed;
    }

    public async Task<HotelFeed> GetHotelFeedAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_hotelFeedCache is not null && DateTime.UtcNow < _hotelExpiry)
                return _hotelFeedCache;
        }

        var feed = await _leblepito.FetchHotelAsync(ct);
        lock (_lock)
        {
            _hotelFeedCache = feed;
            _hotelExpiry = DateTime.UtcNow.Add(CacheTtl);
        }
        return feed;
    }
}
