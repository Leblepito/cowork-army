using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.DataBridge;

namespace CoworkArmy.Infrastructure.DataBridge;

/// <summary>Background polling service — fetches feeds on interval and broadcasts via SignalR.</summary>
public class DataBridgeBackgroundService : BackgroundService
{
    private readonly IDataBridgeService _bridge;
    private readonly IRealtimeNotifier _notifier;
    private readonly ILogger<DataBridgeBackgroundService> _log;
    private readonly int _tradeIntervalMs;
    private readonly int _medicalIntervalMs;
    private readonly int _hotelIntervalMs;

    public DataBridgeBackgroundService(
        IDataBridgeService bridge,
        IRealtimeNotifier notifier,
        IConfiguration config,
        ILogger<DataBridgeBackgroundService> log)
    {
        _bridge = bridge;
        _notifier = notifier;
        _log = log;
        _tradeIntervalMs = config.GetValue("DataBridge:TradeIntervalMs", 5000);
        _medicalIntervalMs = config.GetValue("DataBridge:MedicalIntervalMs", 30000);
        _hotelIntervalMs = config.GetValue("DataBridge:HotelIntervalMs", 30000);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("DataBridge background service starting — trade {T}ms, medical {M}ms, hotel {H}ms",
            _tradeIntervalMs, _medicalIntervalMs, _hotelIntervalMs);

        var tradeTask = PollTradeAsync(ct);
        var medicalTask = PollMedicalAsync(ct);
        var hotelTask = PollHotelAsync(ct);

        await Task.WhenAll(tradeTask, medicalTask, hotelTask);
    }

    private async Task PollTradeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var feed = await _bridge.GetTradeFeedAsync(ct);
                await _notifier.SendTradeFeedAsync(feed);
                _log.LogDebug("Trade feed broadcast: BTC={Btc} ETH={Eth}", feed.BtcPrice, feed.EthPrice);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogWarning(ex, "Trade feed poll error");
            }

            await Task.Delay(_tradeIntervalMs, ct);
        }
    }

    private async Task PollMedicalAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var feed = await _bridge.GetMedicalFeedAsync(ct);
                await _notifier.SendMedicalFeedAsync(feed);
                _log.LogDebug("Medical feed broadcast: patients={P}", feed.PatientsToday);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogWarning(ex, "Medical feed poll error");
            }

            await Task.Delay(_medicalIntervalMs, ct);
        }
    }

    private async Task PollHotelAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var feed = await _bridge.GetHotelFeedAsync(ct);
                await _notifier.SendHotelFeedAsync(feed);
                _log.LogDebug("Hotel feed broadcast: occupancy={O}%", feed.OccupancyPercent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogWarning(ex, "Hotel feed poll error");
            }

            await Task.Delay(_hotelIntervalMs, ct);
        }
    }
}
