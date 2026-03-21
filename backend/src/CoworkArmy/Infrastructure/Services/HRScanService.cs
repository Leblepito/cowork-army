using CoworkArmy.Application.HR;

namespace CoworkArmy.Infrastructure.Services;

public class HRScanService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<HRScanService> _log;

    public HRScanService(IServiceProvider sp, ILogger<HRScanService> log) { _sp = sp; _log = log; }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(10_000, ct); // Wait for startup
        _log.LogInformation("HR scan service started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var scanner = scope.ServiceProvider.GetRequiredService<HRScanHandler>();
                var proposals = await scanner.ScanAsync();
                if (proposals.Count > 0)
                    _log.LogInformation("HR scan: {Count} new proposals", proposals.Count);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "HR scan error");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), ct); // Scan every 5 minutes
        }
    }
}
