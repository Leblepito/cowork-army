using CoworkArmy.Application.CommandChain;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Commands;
using CoworkArmy.Domain.Events;

namespace CoworkArmy.Infrastructure.Services;

public class AutonomousService : BackgroundService, IAutonomousEngine
{
    private readonly IServiceProvider _sp;
    private readonly IStatusTracker _tracker;
    private readonly ILogger<AutonomousService> _log;
    private int _tickCount;
    private bool _running = true;
    private DateTime _lastOrder = DateTime.MinValue;

    public bool Running => _running;
    public int TickCount => _tickCount;
    public void Start() => _running = true;
    public void Stop() => _running = false;

    public AutonomousService(IServiceProvider sp, IStatusTracker tracker, ILogger<AutonomousService> log)
    { _sp = sp; _tracker = tracker; _log = log; }

    private static readonly CeoOrder[] Orders =
    {
        new("Q4 hedeflerini belirleyin", "trade", "trade-master", "Aylık hacim %20 artmalı",
            new[] { ("chart-eye","Yeni trend analizi çıkart"), ("risk-guard","Portföy riskini hesapla"), ("quant-brain","Monte Carlo simülasyonu çalıştır") }),
        new("Medikal turizm kampanyası başlatın", "medical", "clinic-dir", "Rusya segmenti için paket hazırla",
            new[] { ("patient-care","VIP hasta protokolü güncelle") }),
        new("Otel doluluk %90'a çıksın", "hotel", "hotel-mgr", "Yaz sezonu stratejisini uygulayın",
            new[] { ("travel-plan","Charter uçuş anlaşması kur"), ("concierge","Premium tur paketi oluştur") }),
        new("Platform özelliklerini hızlandırın", "software", "tech-lead", "Sprint hızını 2x'e çıkarın",
            new[] { ("full-stack","Dashboard v2 geliştir"), ("data-ops","Analytics pipeline kur"), ("debugger","Performance audit çalıştır") }),
        new("Kripto portföyü dengeleyin", "trade", "trade-master", "BTC/ETH oranını 60/40 yap",
            new[] { ("chart-eye","ETH 4h destek seviyesi bul"), ("risk-guard","Rebalance risk değerlendir") }),
        new("Ameliyat takvimini optimize edin", "medical", "clinic-dir", "Türkiye partner kapasitesini kontrol et",
            new[] { ("patient-care","Bekleme listesini güncelle") }),
        new("Yeni reklam kampanyası çalıştırın", "software", "tech-lead", "Landing page A/B testi kur",
            new[] { ("data-ops","Google Ads metrikleri topla"), ("full-stack","Yeni landing page deploy et") }),
        new("Kış erken rezervasyonu açın", "hotel", "hotel-mgr", "Early-bird %15 indirim uygula",
            new[] { ("travel-plan","Havayolu early-bird fiyat al"), ("concierge","Kış aktivite listesi hazırla") }),
    };

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(2000, ct);
        _log.LogInformation("Autonomous loop started");

        while (!ct.IsCancellationRequested)
        {
            if (_running) { _tickCount++; try { await Tick(); } catch (Exception ex) { _log.LogWarning(ex, "Autonomous tick error"); } }
            await Task.Delay(3000, ct);
        }
    }

    private async Task Tick()
    {
        var rand = Random.Shared;

        // Solo work for random worker
        if (rand.NextDouble() < 0.25)
        {
            var workers = AgentRegistrySeeder.BaseAgents.Where(a => a.Tier == Domain.Agents.AgentTier.WRK).ToArray();
            var a = workers[rand.Next(workers.Length)];
            var st = _tracker.Get(a.Id);
            if (st.Status == "idle")
            {
                var states = new[] { "thinking", "working", "coding", "searching" };
                var newSt = states[rand.Next(states.Length)];
                _tracker.Set(a.Id, newSt, $"[AUTO] {newSt}");

                using var scope = _sp.CreateScope();
                var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
                await notifier.SendStatusChangeAsync(a.Id, newSt);
                await notifier.SendEventAsync("work", a.Id, $"{a.Icon} {a.Name} → {newSt}");

                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000 + rand.Next(4000));
                    _tracker.Set(a.Id, "idle", "[AUTO] idle");
                    using var s = _sp.CreateScope();
                    var n = s.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
                    await n.SendStatusChangeAsync(a.Id, "idle");
                });
            }
        }

        // CEO command chain
        if ((DateTime.UtcNow - _lastOrder).TotalSeconds > 18 && rand.NextDouble() < 0.4)
        {
            _lastOrder = DateTime.UtcNow;
            var order = Orders[rand.Next(Orders.Length)];

            using var scope = _sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<RunCommandChainHandler>();
            _ = handler.ExecuteAsync(order);
        }

        // Cross-department sync beam
        if (rand.NextDouble() < 0.1)
        {
            var workers = AgentRegistrySeeder.BaseAgents.Where(a => a.Tier == Domain.Agents.AgentTier.WRK).ToArray();
            var a1 = workers[rand.Next(workers.Length)];
            var a2 = workers[rand.Next(workers.Length)];
            if (a1.Id != a2.Id && a1.Dept != a2.Dept)
            {
                using var scope = _sp.CreateScope();
                var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
                await notifier.SendEventAsync("sync", a1.Id, $"{a1.Icon}→{a2.Icon} data sync");
            }
        }
    }
}
