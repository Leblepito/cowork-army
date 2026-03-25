using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CoworkArmy.Application.Agents.Commands;
using CoworkArmy.Application.Agents.Queries;
using CoworkArmy.Application.CommandChain;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Application.Tasks.Commands;
using CoworkArmy.Domain;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.HR;
using CoworkArmy.Domain.Messages;
using CoworkArmy.Domain.Tasks;
using CoworkArmy.Domain.Tools;
using CoworkArmy.Infrastructure.Auth;
using CoworkArmy.Infrastructure.Messaging;
using CoworkArmy.Infrastructure.Persistence;
using CoworkArmy.Infrastructure.Realtime;
using CoworkArmy.Infrastructure.LLM;
using CoworkArmy.Infrastructure.Services;
using CoworkArmy.Infrastructure.Tools;

namespace CoworkArmy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<CoworkDbContext>(o => o.UseNpgsql(
            config.GetConnectionString("Default")
            ?? "Host=localhost;Database=cowork;Username=cowork;Password=cowork_dev"));

        // Repositories
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IHRProposalRepository, HRProposalRepository>();
        services.AddScoped<IPerformanceRepository, PerformanceRepository>();
        services.AddScoped<IAgentStateRepository, AgentStateRepository>();

        // Real-time
        services.AddSignalR();
        services.AddSingleton<IRealtimeNotifier, SignalRNotifier>();

        // LLM
        services.AddSingleton<AnthropicProvider>();
        services.AddSingleton<GeminiProvider>();
        services.AddSingleton<OpenAIProvider>();
        services.AddSingleton<ILlmProvider>(sp => sp.GetRequiredService<AnthropicProvider>());
        services.AddSingleton<IEnumerable<ILlmProvider>>(sp => new ILlmProvider[]
        {
            sp.GetRequiredService<AnthropicProvider>(),
            sp.GetRequiredService<GeminiProvider>(),
            sp.GetRequiredService<OpenAIProvider>()
        });
        services.AddSingleton<ILlmProviderFactory, LlmProviderFactory>();
        services.AddSingleton<IBudgetGuard, BudgetGuard>();

        // Tools + Registry
        services.AddHttpClient();
        services.AddSingleton<WebSearchTool>();
        services.AddSingleton<FileReadTool>();
        services.AddSingleton<FileWriteTool>();
        services.AddSingleton<CodeExecuteTool>();
        services.AddSingleton<AgentMessageTool>();
        services.AddSingleton<SendNotificationTool>();
        services.AddSingleton<DbQueryTool>();
        services.AddSingleton<ScheduleTaskTool>();

        // Messaging (must be before ToolRegistry so AgentMessageTool can resolve IMessageBus)
        services.AddSingleton<IMessageBus, ChannelMessageBus>();

        services.AddSingleton<ToolRegistry>(sp =>
        {
            var registry = new ToolRegistry();
            registry.Register(sp.GetRequiredService<WebSearchTool>());
            registry.Register(sp.GetRequiredService<FileReadTool>());
            registry.Register(sp.GetRequiredService<FileWriteTool>());
            registry.Register(sp.GetRequiredService<CodeExecuteTool>());
            registry.Register(sp.GetRequiredService<AgentMessageTool>());
            registry.Register(sp.GetRequiredService<SendNotificationTool>());
            registry.Register(sp.GetRequiredService<DbQueryTool>());
            registry.Register(sp.GetRequiredService<ScheduleTaskTool>());
            return registry;
        });

        // Data Bridge
        services.AddHttpClient<CoworkArmy.Infrastructure.DataBridge.U2AlgoClient>(c =>
            c.BaseAddress = new Uri(config["DataBridge:U2AlgoBaseUrl"] ?? "https://u2algo.com"));
        services.AddHttpClient<CoworkArmy.Infrastructure.DataBridge.LebLepitoClient>(c =>
            c.BaseAddress = new Uri(config["DataBridge:LeblepitoBaseUrl"] ?? "https://leblepito.com"));
        services.AddSingleton<CoworkArmy.Domain.DataBridge.IDataBridgeService, CoworkArmy.Infrastructure.DataBridge.DataBridgeService>();
        services.AddScoped<CoworkArmy.Application.DataBridge.Queries.GetLiveFeedQueryHandler>();
        services.AddHostedService<CoworkArmy.Infrastructure.DataBridge.DataBridgeBackgroundService>();

        // Services
        services.AddSingleton<IStatusTracker, StatusTracker>();
        services.AddSingleton<ITaskRouter, TaskRouter>();
        services.AddHostedService<AutonomousService>();
        services.AddSingleton<IAutonomousEngine>(sp =>
            sp.GetServices<IHostedService>().OfType<AutonomousService>().First());

        // Orchestration
        services.AddScoped<CoworkArmy.Application.Orchestration.AgentOrchestrator>();
        services.AddScoped<CoworkArmy.Application.Orchestration.CommandChainOrchestrator>();

        // Application handlers
        services.AddScoped<SpawnAgentCommandHandler>();
        services.AddScoped<KillAgentCommandHandler>();
        services.AddScoped<CreateAgentCommandHandler>();
        services.AddScoped<GetAgentsQueryHandler>();
        services.AddScoped<CreateTaskCommandHandler>();
        services.AddScoped<RunCommandChainHandler>();
        services.AddScoped<CoworkArmy.Application.Chat.ChatHandler>();

        // Chat persistence
        services.AddScoped<CoworkArmy.Domain.Chat.IChatRepository, CoworkArmy.Infrastructure.Persistence.ChatRepository>();
        services.AddScoped<CoworkArmy.Application.Chat.Queries.GetConversationsQueryHandler>();

        // Claude Bridge
        services.AddScoped<CoworkArmy.Domain.ClaudeBridge.IClaudeBridgeRepository, CoworkArmy.Infrastructure.ClaudeBridge.ClaudeBridgeRepository>();
        services.AddScoped<CoworkArmy.Application.ClaudeBridge.RecordClaudeEventHandler>();
        services.AddScoped<CoworkArmy.Application.ClaudeBridge.StartClaudeTaskHandler>();
        services.AddScoped<CoworkArmy.Application.ClaudeBridge.CompleteClaudeTaskHandler>();
        services.AddScoped<CoworkArmy.Application.ClaudeBridge.GetClaudeTasksHandler>();
        services.AddScoped<CoworkArmy.Application.ClaudeBridge.GetClaudeEventsHandler>();

        // HR
        services.AddScoped<CoworkArmy.Application.HR.HRSpawnHandler>();
        services.AddScoped<CoworkArmy.Application.HR.HRRetireHandler>();
        services.AddScoped<CoworkArmy.Application.HR.HRScanHandler>();
        services.AddScoped<CoworkArmy.Application.HR.HRProposalHandler>();
        services.AddHostedService<HRScanService>();
        services.AddHostedService<EventCleanupService>();

        // Auth — JWT
        services.AddSingleton<JwtService>();
        var jwtService = new JwtService(config);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtService.Issuer,
                    ValidAudience = jwtService.Audience,
                    IssuerSigningKey = jwtService.GetSecurityKey()
                };
            });
        services.AddAuthorization();

        // CORS
        services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (env == "Production")
            {
                var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',')
                    ?? new[] { "https://ireska.com", "https://www.ireska.com" };
                p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }
            else
            {
                p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }
        }));

        return services;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoworkDbContext>();
        // Try migrations first, fall back to EnsureCreated for initial setup
        try
        {
            await db.Database.MigrateAsync();
        }
        catch
        {
            await db.Database.EnsureCreatedAsync();
        }

        // Seed base agents
        foreach (var agent in AgentRegistrySeeder.CreateAll())
        {
            if (!await db.Agents.AnyAsync(a => a.Id == agent.Id))
                db.Agents.Add(agent);
        }
        await db.SaveChangesAsync();

        // Seed agent states
        foreach (var a in AgentRegistrySeeder.BaseAgents)
        {
            if (!await db.AgentStates.AnyAsync(s => s.AgentId == a.Id))
                db.AgentStates.Add(new Domain.Agents.AgentState { AgentId = a.Id });
        }
        await db.SaveChangesAsync();

        // Set immortality for CEO and HR Agent
        var ceo = await db.Agents.FindAsync("ceo");
        if (ceo != null && !ceo.IsImmortal) { ceo.SetImmortal(); }
        var hrAgent = await db.Agents.FindAsync("hr-agent");
        if (hrAgent != null && !hrAgent.IsImmortal) { hrAgent.SetImmortal(); }
        await db.SaveChangesAsync();

        // Recovery: mark running tasks as failed on restart
        var runningTasks = await db.Tasks.Where(t => t.Status == Domain.Tasks.TaskStatus.Running).ToListAsync();
        foreach (var task in runningTasks) task.Fail("server_restart");
        if (runningTasks.Count > 0) await db.SaveChangesAsync();

        // Init status tracker
        var tracker = scope.ServiceProvider.GetRequiredService<IStatusTracker>();
        tracker.Init(AgentRegistrySeeder.BaseAgents.Select(a => a.Id));
    }
}
