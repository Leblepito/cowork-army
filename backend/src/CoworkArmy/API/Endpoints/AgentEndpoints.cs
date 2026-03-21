using CoworkArmy.Application.Agents.Commands;
using CoworkArmy.Application.Agents.DTOs;
using CoworkArmy.Application.Agents.Queries;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;

namespace CoworkArmy.API.Endpoints;

public static class AgentEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/agents").WithTags("Agents");

        g.MapGet("/", async (GetAgentsQueryHandler handler) =>
            Results.Ok(await handler.HandleAsync()));

        g.MapGet("/{id}", async (string id, GetAgentsQueryHandler handler) =>
        {
            var dto = await handler.HandleByIdAsync(id);
            return dto is not null ? Results.Ok(dto) : Results.NotFound();
        });

        g.MapPost("/", async (AgentCreateDto dto, CreateAgentCommandHandler handler) =>
        {
            var result = await handler.HandleAsync(new CreateAgentCommand(dto));
            return Results.Created($"/api/agents/{result.Id}", result);
        });

        g.MapDelete("/{id}", async (string id, IAgentRepository repo) =>
        {
            var agent = await repo.GetByIdAsync(id);
            if (agent == null) return Results.NotFound();
            if (agent.IsBase) return Results.BadRequest(new { error = "Cannot delete base agent" });
            await repo.DeleteAsync(id);
            return Results.Ok(new { deleted = true, id });
        });

        g.MapGet("/{id}/status", (string id, IStatusTracker tracker) =>
            Results.Ok(tracker.Get(id)));

        g.MapGet("/{id}/output", (string id, IStatusTracker tracker) =>
            Results.Ok(new { lines = tracker.Get(id).Lines }));

        g.MapPost("/{id}/spawn", async (string id, string? task, SpawnAgentCommandHandler handler) =>
            Results.Ok(await handler.HandleAsync(new SpawnAgentCommand(id, task))));

        g.MapPost("/{id}/kill", async (string id, KillAgentCommandHandler handler) =>
        {
            await handler.HandleAsync(new KillAgentCommand(id));
            return Results.Ok(new { status = "killed", id });
        });
    }
}
