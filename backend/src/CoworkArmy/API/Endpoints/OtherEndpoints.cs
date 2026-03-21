using CoworkArmy.Application.Interfaces;
using CoworkArmy.Application.Orchestration;
using CoworkArmy.Application.Tasks.Commands;
using CoworkArmy.Application.Tasks.DTOs;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CoworkArmy.API.Endpoints;

// ═══ TASKS ═══
public static class TaskEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/tasks").WithTags("Tasks");

        g.MapGet("/", async (ITaskRepository repo, int? limit, int? offset) =>
        {
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var query = await repo.GetRecentAsync(take, offset.HasValue && offset.Value > 0 ? offset.Value : 0);
            return Results.Ok(query);
        });

        g.MapPost("/", async (TaskCreateDto dto, CreateTaskCommandHandler handler) =>
        {
            var result = await handler.HandleAsync(new CreateTaskCommand(dto));
            return Results.Created($"/api/tasks/{result.Id}", result);
        });
    }
}

// ═══ COMMANDER ═══
public static class CommandEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/commander/delegate", (DelegateDto dto, ITaskRouter router) =>
        {
            var agent = router.Route(dto.Title + " " + dto.Description);
            return Results.Ok(new { routed_to = agent, title = dto.Title });
        }).WithTags("Commander");

        app.MapGet("/api/statuses", (IStatusTracker tracker) =>
            Results.Ok(tracker.GetAll())).WithTags("Status");
    }
}

// ═══ AUTONOMOUS ═══
public static class AutonomousEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/autonomous").WithTags("Autonomous");

        g.MapGet("/status", (IAutonomousEngine engine, IStatusTracker tracker) =>
            Results.Ok(new
            {
                running = engine.Running,
                tick_count = engine.TickCount,
                agents_tracked = tracker.GetAll().Count,
            }));

        g.MapPost("/start", (IAutonomousEngine engine) =>
        {
            engine.Start();
            return Results.Ok(new { status = "started" });
        });

        g.MapPost("/stop", (IAutonomousEngine engine) =>
        {
            engine.Stop();
            return Results.Ok(new { status = "stopped" });
        });
    }
}

// ═══ EVENTS ═══
public static class EventEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/events", async (IEventRepository repo, int? limit, int? offset) =>
        {
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var skip = offset.HasValue && offset.Value > 0 ? offset.Value : 0;
            return Results.Ok(await repo.GetRecentAsync(take, skip));
        }).WithTags("Events");
    }
}

// ═══ SETTINGS ═══
public static class SettingsEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/settings").WithTags("Settings");

        g.MapGet("/api-key-status", () => Results.Ok(new
        {
            set = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")),
            active_provider = Environment.GetEnvironmentVariable("LLM_PROVIDER") ?? "anthropic"
        }));

        g.MapGet("/llm-provider", () =>
            Results.Ok(new { provider = Environment.GetEnvironmentVariable("LLM_PROVIDER") ?? "anthropic" }));

        g.MapPost("/llm-provider", (ProviderDto dto) =>
        {
            Environment.SetEnvironmentVariable("LLM_PROVIDER", dto.Provider);
            return Results.Ok(new { status = "ok", provider = dto.Provider });
        });
    }
}

public record ProviderDto(string Provider);

// ═══ CHAT ═══
public static class ChatEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api").WithTags("Chat");

        // Simple chat (backward compatible)
        g.MapPost("/chat/{agentId}", async (string agentId, ChatMessageDto dto,
            CoworkArmy.Application.Chat.ChatHandler handler) =>
        {
            if (string.IsNullOrWhiteSpace(agentId))
                return Results.BadRequest(new { error = "agentId is required" });
            var result = await handler.HandleAsync(new CoworkArmy.Application.Chat.ChatCommand(agentId, dto.Message));
            return Results.Ok(new { response = result.Response, tokens = result.Tokens, cost = result.Cost });
        });

        // Chat with conversation persistence + Data Bridge context
        g.MapPost("/agents/{agentId}/chat", async (string agentId, ChatWithConversationDto dto,
            CoworkArmy.Application.Chat.ChatHandler handler) =>
        {
            if (string.IsNullOrWhiteSpace(agentId))
                return Results.BadRequest(new { error = "agentId is required" });
            var result = await handler.HandleWithConversationAsync(
                new CoworkArmy.Application.Chat.DTOs.SendMessageDto(agentId, dto.Message, dto.ConversationId));
            return Results.Ok(result);
        });

        // Get conversations for an agent
        g.MapGet("/agents/{agentId}/conversations", async (string agentId, int? limit,
            CoworkArmy.Application.Chat.Queries.GetConversationsQueryHandler handler) =>
        {
            var convs = await handler.HandleByAgentAsync(agentId, limit ?? 20);
            return Results.Ok(convs);
        });

        // Get single conversation
        g.MapGet("/conversations/{convId}", async (string convId,
            CoworkArmy.Application.Chat.Queries.GetConversationsQueryHandler handler) =>
        {
            var conv = await handler.HandleByIdAsync(convId);
            return conv is not null ? Results.Ok(conv) : Results.NotFound();
        });
    }
}

public record ChatMessageDto(string Message);
public record ChatWithConversationDto(string Message, string? ConversationId = null);
public record ChainDto(string Message);

// ═══ BUDGET ═══
public static class BudgetEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/budget/status", async (CoworkArmy.Application.Interfaces.IBudgetGuard budget) =>
            Results.Ok(await budget.GetStatusAsync())).WithTags("Budget");
    }
}

// ═══ TOOLS ═══
public static class ToolEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/tools", (CoworkArmy.Domain.Tools.ToolRegistry registry) =>
            Results.Ok(registry.GetAll().Select(t => new
            {
                name = t.Name, description = t.Description,
                permission = t.Permission.ToString(), requiredParams = t.RequiredParams
            }))).WithTags("Tools");
    }
}

// ═══ MESSAGES ═══
public static class MessageEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/agents/{id}/messages", async (string id,
            CoworkArmy.Infrastructure.Persistence.CoworkDbContext db, int? limit, int? offset) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest(new { error = "agentId is required" });
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var query = db.AgentMessages
                .Where(m => m.FromId == id || m.ToId == id)
                .OrderByDescending(m => m.Timestamp);
            if (offset.HasValue && offset.Value > 0)
                return Results.Ok(await query.Skip(offset.Value).Take(take).ToListAsync());
            return Results.Ok(await query.Take(take).ToListAsync());
        }).WithTags("Messages");
    }
}

// ═══ ORCHESTRATE ═══
public static class OrchestrateEndpoints
{
    private static readonly ConcurrentDictionary<string, ChainStatus> _chainStatuses = new();
    public record ChainStatus(string Id, string Status, string? Error, DateTime StartedAt);

    public static void Map(WebApplication app)
    {
        app.MapPost("/api/orchestrate/{agentId}", async (string agentId, ChatMessageDto dto,
            CoworkArmy.Application.Orchestration.AgentOrchestrator orchestrator) =>
        {
            if (string.IsNullOrWhiteSpace(agentId))
                return Results.BadRequest(new { error = "agentId is required" });
            var result = await orchestrator.ThinkAndActAsync(agentId, dto.Message);
            return Results.Ok(new { response = result });
        }).WithTags("Orchestrate");

        app.MapPost("/api/commander/chain", async (ChainDto dto,
            CoworkArmy.Application.Orchestration.CommandChainOrchestrator chain) =>
        {
            var chainId = Guid.NewGuid().ToString("N")[..8];
            _chainStatuses[chainId] = new ChainStatus(chainId, "running", null, DateTime.UtcNow);

            _ = Task.Run(async () =>
            {
                try
                {
                    await chain.ExecuteAsync(dto.Message);
                    _chainStatuses[chainId] = _chainStatuses[chainId] with { Status = "completed" };
                }
                catch (Exception ex)
                {
                    _chainStatuses[chainId] = _chainStatuses[chainId] with { Status = "failed", Error = ex.Message };
                }
            });

            return Results.Ok(new { chainId, status = "started" });
        }).WithTags("Commander");

        app.MapGet("/api/commander/chain/{chainId}/status", (string chainId) =>
        {
            if (_chainStatuses.TryGetValue(chainId, out var status))
                return Results.Ok(status);
            return Results.NotFound(new { error = "Chain not found" });
        }).WithTags("Commander");
    }
}
