using CoworkArmy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoworkArmy.Infrastructure.Services;

public class EventCleanupService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<EventCleanupService> _logger;

    public EventCleanupService(IServiceProvider provider, ILogger<EventCleanupService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            try
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CoworkDbContext>();
                var cutoff = DateTime.UtcNow.AddDays(-30);

                var deleted = await db.Events
                    .Where(e => e.Timestamp < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                    _logger.LogInformation("Cleaned up {Count} events older than 30 days", deleted);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Event cleanup failed");
            }
        }
    }
}
