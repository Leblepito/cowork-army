using System.Text.Json.Serialization;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Events;

namespace CoworkArmy.API.Endpoints;

public static class ExternalEndpoints
{
    // Valid u2Algo event types
    private static readonly HashSet<string> ValidEventTypes = new()
    {
        "bot_started", "bot_stopped", "trade_opened", "trade_closed",
        "trade_tp1", "hedge_opened", "hedge_closed", "agent_message", "status_change",
        "intake_submitted", "hospital_matched", "commission_calculated",
        "quote_generated", "booking_confirmed", "content_generated",
        "campaign_created", "lead_scored", "notification_sent",
        "patient_status_updated", "followup_checkin"
    };

    // Map u2Algo event types to agent statuses
    private static readonly Dictionary<string, string> StatusMap = new()
    {
        ["bot_started"] = "working",
        ["bot_stopped"] = "idle",
        ["trade_opened"] = "working",
        ["trade_closed"] = "idle",
        ["trade_tp1"] = "working",
        ["hedge_opened"] = "working",
        ["hedge_closed"] = "idle",
        ["status_change"] = "working",
        ["intake_submitted"] = "working",
        ["hospital_matched"] = "working",
        ["commission_calculated"] = "idle",
        ["quote_generated"] = "idle",
        ["booking_confirmed"] = "working",
        ["content_generated"] = "idle",
        ["campaign_created"] = "working",
        ["lead_scored"] = "idle",
        ["notification_sent"] = "idle",
        ["patient_status_updated"] = "working",
        ["followup_checkin"] = "working",
    };

    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/external").WithTags("External");

        // POST /api/external/event -- receive events from u2Algo or other external systems
        g.MapPost("/event", async (HttpContext context, ExternalEventDto dto,
            IStatusTracker tracker,
            IRealtimeNotifier notifier,
            IEventRepository events) =>
        {
            // API key validation
            var expectedKey = Environment.GetEnvironmentVariable("EXTERNAL_API_KEY");
            if (!string.IsNullOrEmpty(expectedKey))
            {
                var providedKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
                if (providedKey != expectedKey)
                    return Results.Unauthorized();
            }

            // Length validation
            if (dto.Source?.Length > 100 || dto.EventType?.Length > 100)
                return Results.BadRequest(new { error = "Field too long (max 100 chars)" });
            if (dto.AgentId?.Length > 100)
                return Results.BadRequest(new { error = "agent_id too long (max 100 chars)" });

            // Validate
            if (string.IsNullOrEmpty(dto.Source) || string.IsNullOrEmpty(dto.EventType))
                return Results.BadRequest(new { error = "source and event_type required" });

            if (!ValidEventTypes.Contains(dto.EventType))
                return Results.BadRequest(new { error = $"Invalid event_type: {dto.EventType}" });

            // 1. Update agent status if applicable
            if (!string.IsNullOrEmpty(dto.AgentId) && StatusMap.TryGetValue(dto.EventType, out var newStatus))
            {
                tracker.Set(dto.AgentId, newStatus, $"[{dto.Source}] {dto.EventType}");
                await notifier.SendStatusChangeAsync(dto.AgentId, newStatus);
            }

            // 2. Build display message
            var displayMsg = BuildDisplayMessage(dto);

            // 3. Save to events DB
            var agentId = dto.AgentId ?? dto.Source;
            await events.AddAsync(new AgentEvent
            {
                Type = MapEventTypeToDbType(dto.EventType),
                AgentId = agentId,
                Message = $"[{dto.Source}] {displayMsg}"
            });

            // 4. Broadcast via SignalR
            var icon = GetEventIcon(dto.EventType);
            await notifier.SendEventAsync(
                MapEventTypeToDbType(dto.EventType),
                agentId,
                $"{icon} [{dto.Source}] {displayMsg}");

            return Results.Ok(new { received = true, source = dto.Source, event_type = dto.EventType });
        });
    }

    private static string MapEventTypeToDbType(string eventType) => eventType switch
    {
        "bot_started" or "bot_stopped" => "info",
        "trade_opened" or "trade_closed" or "trade_tp1" => "work",
        "hedge_opened" or "hedge_closed" => "work",
        "agent_message" => "message",
        "status_change" => "info",
        "intake_submitted" or "hospital_matched" or "patient_status_updated" or "followup_checkin" => "work",
        "commission_calculated" or "quote_generated" or "lead_scored" => "complete",
        "booking_confirmed" or "campaign_created" => "work",
        "content_generated" or "notification_sent" => "info",
        _ => "info",
    };

    private static string GetEventIcon(string eventType) => eventType switch
    {
        "trade_opened" => "\U0001F4C8",
        "trade_closed" => "\U0001F4C9",
        "trade_tp1" => "\U0001F3AF",
        "hedge_opened" or "hedge_closed" => "\U0001F6E1\uFE0F",
        "bot_started" or "bot_stopped" => "\U0001F916",
        "agent_message" => "\U0001F4AC",
        "status_change" => "\U0001F504",
        "intake_submitted" => "🏥",
        "hospital_matched" => "🤝",
        "commission_calculated" => "💰",
        "quote_generated" => "🏭",
        "booking_confirmed" => "✈️",
        "content_generated" => "📝",
        "campaign_created" => "📣",
        "lead_scored" => "🎯",
        "notification_sent" => "📨",
        "patient_status_updated" => "📋",
        "followup_checkin" => "🩺",
        _ => "\U0001F4E1",
    };

    private static string BuildDisplayMessage(ExternalEventDto dto)
    {
        var parts = new List<string> { dto.EventType };
        if (dto.Data != null)
        {
            if (dto.Data.TryGetValue("symbol", out var symbol))
                parts.Add(symbol.ToString()!);
            if (dto.Data.TryGetValue("direction", out var dir))
                parts.Add(dir.ToString()!);
            if (dto.Data.TryGetValue("entry_price", out var price))
                parts.Add($"@{price}");
        }
        return string.Join(" ", parts);
    }
}

// DTO for external events
public record ExternalEventDto(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("agent_id")] string? AgentId,
    [property: JsonPropertyName("data")] Dictionary<string, object>? Data,
    [property: JsonPropertyName("timestamp")] DateTime? Timestamp
);
