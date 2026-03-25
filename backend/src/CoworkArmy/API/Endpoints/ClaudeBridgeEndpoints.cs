using CoworkArmy.Application.ClaudeBridge;
using CoworkArmy.Application.ClaudeBridge.DTOs;

namespace CoworkArmy.API.Endpoints;

public static class ClaudeBridgeEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/claude-bridge").WithTags("ClaudeBridge");
        g.MapPost("/events", RecordEvent);
        g.MapPost("/tasks/start", StartTask);
        g.MapPost("/tasks/{id}/complete", CompleteTask);
        g.MapGet("/tasks", GetTasks);
        g.MapGet("/events", GetEvents);
    }

    private static async Task<IResult> RecordEvent(RecordClaudeEventDto dto, RecordClaudeEventHandler handler)
    {
        var ev = await handler.HandleAsync(dto);
        return Results.Ok(new { ev.Id, ev.AgentId, ev.Tool });
    }

    private static async Task<IResult> StartTask(StartClaudeTaskDto dto, StartClaudeTaskHandler handler)
    {
        var task = await handler.HandleAsync(dto);
        return Results.Ok(new { task.Id, task.Title, task.Scope });
    }

    private static async Task<IResult> CompleteTask(string id, CompleteClaudeTaskDto dto, CompleteClaudeTaskHandler handler)
    {
        var task = await handler.HandleAsync(id, dto);
        return task is null ? Results.NotFound() : Results.Ok(new { task.Id, task.Status });
    }

    private static async Task<IResult> GetTasks(GetClaudeTasksHandler handler)
        => Results.Ok(await handler.HandleAsync());

    private static async Task<IResult> GetEvents(GetClaudeEventsHandler handler, int limit = 50)
        => Results.Ok(await handler.HandleAsync(limit));
}
