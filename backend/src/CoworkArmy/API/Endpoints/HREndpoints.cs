using Microsoft.EntityFrameworkCore;
using CoworkArmy.Application.HR;
using CoworkArmy.Application.HR.DTOs;

namespace CoworkArmy.API.Endpoints;

public static class HREndpoints
{
    private static readonly HashSet<string> ValidDepartments = new(StringComparer.OrdinalIgnoreCase)
    {
        "engineering", "marketing", "operations", "finance", "hr", "design", "support", "research"
    };

    public static void Map(WebApplication app)
    {
        // All HR endpoints require a valid JWT bearer token
        var g = app.MapGroup("/api/hr").WithTags("HR").RequireAuthorization();

        // GET /api/hr/performance — all agent performance metrics
        g.MapGet("/performance", async (CoworkArmy.Infrastructure.Persistence.CoworkDbContext db) =>
            Results.Ok(await db.AgentPerformance.Select(p => new PerformanceDto(
                p.AgentId, p.TasksCompleted, p.TasksFailed, p.AvgResponseMs,
                p.TotalTokens, p.EstimatedCost, p.Warnings, p.Grade, p.LastActiveAt
            )).ToListAsync()));

        // POST /api/hr/spawn — HR designs and creates a new agent
        g.MapPost("/spawn", async (SpawnRequestDto dto, HRSpawnHandler handler) =>
        {
            if (!ValidDepartments.Contains(dto.Department))
                return Results.BadRequest(new { error = $"Invalid department: {dto.Department}" });

            var result = await handler.HandleAsync(dto.Reason, dto.Department);
            return Results.Created($"/api/agents/{result.AgentId}", result);
        });

        // POST /api/hr/retire/{agentId} — retire an agent
        g.MapPost("/retire/{agentId}", async (string agentId, RetireRequestDto dto, HRRetireHandler handler) =>
        {
            await handler.HandleAsync(agentId, dto.Reason);
            return Results.Ok(new { retired = true, agentId });
        });

        // POST /api/hr/warn/{agentId} — issue warning
        g.MapPost("/warn/{agentId}", async (string agentId, WarnRequestDto dto,
            CoworkArmy.Infrastructure.Persistence.CoworkDbContext db) =>
        {
            var perf = await db.AgentPerformance.FindAsync(agentId);
            if (perf == null) return Results.NotFound();
            perf.Warnings++;
            await db.SaveChangesAsync();
            return Results.Ok(new { warnings = perf.Warnings, agentId });
        });

        // GET /api/hr/proposals — pending proposals
        g.MapGet("/proposals", async (HRProposalHandler handler, int? limit, int? offset) =>
        {
            var all = await handler.GetPendingAsync();
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var skip = offset.HasValue && offset.Value > 0 ? offset.Value : 0;
            return Results.Ok(all.Skip(skip).Take(take).ToList());
        });

        // POST /api/hr/proposals/{id}/approve
        g.MapPost("/proposals/{id}/approve", async (string id, HRProposalHandler handler) =>
        {
            await handler.ApproveAsync(id);
            return Results.Ok(new { executed = true, id });
        });
    }
}
