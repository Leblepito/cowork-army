using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;

namespace CoworkArmy.Infrastructure.Services;

// ═══ In-memory status tracker ═══
public class StatusTracker : IStatusTracker
{
    private readonly Dictionary<string, AgentStatus> _statuses = new();

    public AgentStatus Get(string id) =>
        _statuses.TryGetValue(id, out var s) ? s : new(id);

    public Dictionary<string, AgentStatus> GetAll() => new(_statuses);

    public void Set(string id, string status, string? line = null)
    {
        if (!_statuses.ContainsKey(id))
            _statuses[id] = new(id);

        _statuses[id] = _statuses[id] with
        {
            Status = status,
            Alive = status != "idle",
            StartedAt = status != "idle" ? DateTime.UtcNow : null
        };

        if (line != null) _statuses[id].Lines.Add(line);
        if (_statuses[id].Lines.Count > 50)
            _statuses[id].Lines.RemoveRange(0, _statuses[id].Lines.Count - 50);
    }

    public void AddLog(string id, string line)
    {
        if (!_statuses.ContainsKey(id))
            _statuses[id] = new(id);
        _statuses[id].Lines.Add(line);
    }

    public void Init(IEnumerable<string> ids)
    {
        foreach (var id in ids)
            _statuses[id] = new(id);
    }
}

// ═══ Keyword-based task router ═══
public class TaskRouter : ITaskRouter
{
    private static readonly Dictionary<string, string[]> Keywords = new()
    {
        ["trade-master"] = new[] { "trade", "trading", "sinyal", "kripto", "btc", "eth" },
        ["chart-eye"] = new[] { "chart", "grafik", "teknik", "analiz", "fibonacci" },
        ["risk-guard"] = new[] { "risk", "veto", "stop-loss", "drawdown", "zarar" },
        ["quant-brain"] = new[] { "backtest", "optimizasyon", "sharpe", "monte" },
        ["clinic-dir"] = new[] { "medikal", "hasta", "doktor", "klinik", "ameliyat" },
        ["patient-care"] = new[] { "bakım", "post-op", "ilaç", "takip" },
        ["hotel-mgr"] = new[] { "otel", "oda", "doluluk", "check-in" },
        ["travel-plan"] = new[] { "uçuş", "transfer", "seyahat", "tur", "havalimanı" },
        ["concierge"] = new[] { "restoran", "spa", "aktivite", "rezervasyon" },
        ["tech-lead"] = new[] { "sprint", "mimari", "code review", "deploy" },
        ["full-stack"] = new[] { "frontend", "backend", "api", "react", "python" },
        ["data-ops"] = new[] { "veri", "analiz", "seo", "reklam", "metrik" },
        ["debugger"] = new[] { "bug", "hata", "error", "debug", "monitoring" },
        ["trading-director"] = new[] { "trade", "hedge", "bot", "position", "signal", "cooperative", "u2algo" },
        ["ew-smc-bot"] = new[] { "elliott", "wave", "smc", "smart money", "entry", "tp1", "tp2" },
        ["hedge-bot"] = new[] { "hedge", "hedging", "range", "cooperative", "position" },
        ["trade-validator"] = new[] { "validate", "validation", "risk check", "sl", "tp", "leverage" },
        ["code-reviewer"] = new[] { "review", "code quality", "pattern check", "pr" },
        ["medical-coordinator"] = new[] { "hasta", "intake", "hastane", "komisyon", "patient", "hospital", "medical tourism" },
        ["travel-coordinator"] = new[] { "otel", "rezervasyon", "phuket", "transfer", "booking", "accommodation" },
        ["factory-manager"] = new[] { "üretim", "eldiven", "maske", "fabrika", "b2b", "gloves", "manufacturing" },
        ["marketing-chief"] = new[] { "seo", "kampanya", "içerik", "reklam", "lead", "funnel", "content", "campaign" },
        ["medical-secretary"] = new[] { "sekreter", "chat", "sohbet", "randevu", "appointment", "inquiry" },
    };

    public string Route(string text)
    {
        var lower = text.ToLowerInvariant();
        var scores = Keywords
            .Select(kv => (kv.Key, Score: kv.Value.Count(k => lower.Contains(k))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();
        return scores.Count > 0 ? scores[0].Key : "tech-lead";
    }
}

// ═══ Seed 26 base agents ═══
public static class AgentRegistrySeeder
{
    public static readonly (string Id, string Name, string Icon, AgentTier Tier, string Color, string Dept, string Desc, string Skills, string Prompt)[] BaseAgents =
    {
        ("ceo",          "CEO",              "👑", AgentTier.CEO, "#fbbf24", "hq",       "AntiGravity Ventures CEO",    "[\"strategy\",\"leadership\"]",    "Sen CEO. Departmanları yönet."),
        ("hr-agent",    "HR Agent",         "🧑‍💼", AgentTier.DIR, "#10b981", "hr",      "İnsan kaynakları", "[\"hiring\",\"performance\",\"cost_analysis\"]", "Agent performansını izle."),
        ("cargo",        "Cargo Hub",        "📦", AgentTier.SUP, "#f59e0b", "cargo",    "Departmanlar arası teslimat",  "[\"delivery\",\"routing\"]",       "Teslimat koordine et."),
        ("trade-master", "Trade Master",     "📊", AgentTier.DIR, "#f59e0b", "trade",    "Trading müdürü",              "[\"swarm\",\"signals\"]",          "Trading ekibini yönet."),
        ("chart-eye",    "Chart Eye",        "👁️", AgentTier.WRK, "#eab308", "trade",    "Teknik analiz uzmanı",        "[\"chart\",\"pattern\"]",          "Teknik analiz yap."),
        ("risk-guard",   "Risk Guard",       "🛡️", AgentTier.WRK, "#dc2626", "trade",    "Risk yönetimi — VETO",        "[\"risk\",\"veto\"]",              "Risk değerlendir."),
        ("quant-brain",  "Quant Brain",      "🔬", AgentTier.WRK, "#16a34a", "trade",    "Backtest & optimizasyon",     "[\"backtest\",\"sharpe\"]",        "Backtest yap."),
        ("clinic-dir",   "Clinic Director",  "🏥", AgentTier.DIR, "#22d3ee", "medical",  "Medikal müdürü",              "[\"triage\",\"planning\"]",        "Medikal ops yönet."),
        ("patient-care", "Patient Care",     "💊", AgentTier.WRK, "#06b6d4", "medical",  "Hasta bakım hemşiresi",       "[\"post_op\",\"followup\"]",       "Hasta bakım yap."),
        ("hotel-mgr",    "Hotel Manager",    "🏨", AgentTier.DIR, "#ec4899", "hotel",    "Otel müdürü",                 "[\"rooms\",\"operations\"]",       "Otel ops yönet."),
        ("travel-plan",  "Travel Planner",   "✈️", AgentTier.WRK, "#a855f7", "hotel",    "Seyahat planlama",            "[\"flights\",\"tours\"]",          "Uçuş ve tur planla."),
        ("concierge",    "Concierge",        "🛎️", AgentTier.WRK, "#f472b6", "hotel",    "Misafir hizmetleri",          "[\"restaurant\",\"spa\"]",         "Misafir hizmetleri."),
        ("tech-lead",    "Tech Lead",        "💻", AgentTier.DIR, "#a855f7", "software", "Yazılım müdürü",              "[\"architecture\",\"sprint\"]",    "Yazılım ekibini yönet."),
        ("full-stack",   "Full-Stack",       "🔧", AgentTier.WRK, "#6366f1", "software", "Full-stack dev",              "[\"react\",\"python\",\"api\"]",   "Frontend+backend geliştir."),
        ("data-ops",     "Data Ops",         "📈", AgentTier.WRK, "#8b5cf6", "software", "Veri analisti",               "[\"analytics\",\"seo\"]",          "Veri analizi yap."),
        ("debugger",     "Debugger",         "🐛", AgentTier.SUP, "#ef4444", "software", "Hata avcısı",                 "[\"debugging\",\"monitoring\"]",   "Hata ayıkla."),
        ("trading-director","Trading Director","👑",AgentTier.DIR,"#ff4466","trade","u2Algo trading team director","[\"trade\",\"hedge\",\"position\",\"signal\",\"bot\",\"cooperative\"]","u2Algo trading ekibi müdürü."),
        ("ew-smc-bot",      "EW-SMC Trader",   "📊",AgentTier.WRK,"#00ff88","trade","Elliott Wave + SMC trading bot","[\"trade\",\"elliott_wave\",\"smc\",\"signal\"]","EW+SMC stratejisiyle trade yap."),
        ("hedge-bot",       "Hedge Bot",        "🛡️",AgentTier.WRK,"#ffaa00","trade","Range-based cooperative hedge bot","[\"hedge\",\"position\",\"cooperative\",\"risk\"]","Hedge stratejisi uygula."),
        ("trade-validator", "Trade Validator",  "✅",AgentTier.SUP,"#00ccff","trade","Trade logic and risk validator","[\"validation\",\"risk\",\"trade\",\"signal\"]","Trade mantığını doğrula."),
        ("code-reviewer",   "Code Reviewer",    "🔍",AgentTier.SUP,"#cc44ff","trade","Code quality and pattern reviewer","[\"code_review\",\"pattern\",\"quality\"]","Kod kalitesini incele."),
        // ── ThaiTurk Medical Tourism Agents ──
        ("medical-coordinator","Medical Coordinator","🏥",AgentTier.DIR,"#22d3ee","medical","ThaiTurk medikal koordinatör","[\"triage\",\"hospital_match\",\"commission\",\"intake\"]","Hasta intake ve hastane eşleştir."),
        ("travel-coordinator", "Travel Coordinator", "✈️",AgentTier.WRK,"#a855f7","hotel","Phuket otel koordinatörü","[\"booking\",\"pricing\",\"availability\"]","Otel rezervasyonu koordine et."),
        ("factory-manager",    "Factory Manager",    "🏭",AgentTier.WRK,"#78716c","medical","B2B medikal üretim","[\"quote\",\"manufacturing\",\"b2b\"]","B2B medikal teklif oluştur."),
        ("marketing-chief",    "Marketing Chief",    "📣",AgentTier.DIR,"#f43f5e","software","ThaiTurk CMO","[\"seo\",\"content\",\"campaign\",\"analytics\"]","SEO ve kampanya yönet."),
        ("medical-secretary",  "Medical Secretary",  "💬",AgentTier.WRK,"#14b8a6","medical","AI medikal sekreter","[\"chat\",\"tool_use\",\"patient_inquiry\"]","Hasta iletişimi yönet."),
    };

    public static List<Agent> CreateAll()
        => BaseAgents.Select(a => Agent.Create(a.Id, a.Name, a.Icon, a.Tier, a.Color, a.Dept, a.Desc, a.Skills, a.Prompt)).ToList();
}
