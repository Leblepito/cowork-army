using CoworkArmy.API.Endpoints;
using CoworkArmy.API.Middleware;
using CoworkArmy.Infrastructure;
using CoworkArmy.Infrastructure.Realtime;

var builder = WebApplication.CreateBuilder(args);

// ═══ Register all layers via Infrastructure DI ═══
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "COWORK.ARMY API", Version = "v9" });
});

var app = builder.Build();

// ═══ Production safety checks ═══
var env = app.Environment;
if (env.IsProduction())
{
    var connStr = app.Configuration.GetConnectionString("Default");
    if (string.IsNullOrEmpty(connStr) || connStr.Contains("localhost"))
        throw new InvalidOperationException(
            "Production requires a valid ConnectionStrings__Default (not localhost).");
}

if (!env.IsProduction() && !env.IsDevelopment())
    app.Logger.LogWarning("ASPNETCORE_ENVIRONMENT is '{Env}' — expected 'Production' or 'Development'",
        env.EnvironmentName);

// ═══ Middleware ═══
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<AuditMiddleware>();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

// ═══ Seed database ═══
await app.SeedDatabaseAsync();

// ═══ Health check ═══
app.MapGet("/health", () => new { status = "ok", version = "9.0", runtime = "dotnet8", architecture = "DDD" });

app.MapGet("/api/info", (CoworkArmy.Application.Interfaces.IAutonomousEngine auto,
    CoworkArmy.Application.Interfaces.IStatusTracker tracker) => new
{
    name = "COWORK.ARMY",
    version = "9.0",
    mode = "csharp-ddd",
    agents = CoworkArmy.Infrastructure.Services.AgentRegistrySeeder.BaseAgents.Length,
    autonomous = auto.Running,
    autonomous_ticks = auto.TickCount,
});

// ═══ Map all endpoint groups ═══
AgentEndpoints.Map(app);
TaskEndpoints.Map(app);
CommandEndpoints.Map(app);
AutonomousEndpoints.Map(app);
EventEndpoints.Map(app);
SettingsEndpoints.Map(app);

ChatEndpoints.Map(app);
BudgetEndpoints.Map(app);

ToolEndpoints.Map(app);
MessageEndpoints.Map(app);
OrchestrateEndpoints.Map(app);

HREndpoints.Map(app);
AuthEndpoints.Map(app);
ExternalEndpoints.Map(app);
DataBridgeEndpoints.Map(app);

// ═══ SignalR hub ═══
app.MapHub<CoworkHub>("/hub");

// ═══ SPA fallback — serve index.html for unknown routes ═══
app.MapFallbackToFile("index.html");

// ═══ Run ═══
var port = Environment.GetEnvironmentVariable("PORT") ?? "8888";
app.Run($"http://0.0.0.0:{port}");
