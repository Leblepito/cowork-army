using CoworkArmy.Application.DataBridge.Queries;

namespace CoworkArmy.API.Endpoints;

public static class DataBridgeEndpoints
{
    public static void Map(WebApplication app)
    {
        var g = app.MapGroup("/api/bridge").WithTags("DataBridge");

        g.MapGet("/trade", async (GetLiveFeedQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.GetTradeAsync(ct)));

        g.MapGet("/medical", async (GetLiveFeedQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.GetMedicalAsync(ct)));

        g.MapGet("/hotel", async (GetLiveFeedQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.GetHotelAsync(ct)));

        g.MapGet("/all", async (GetLiveFeedQueryHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.GetAllAsync(ct)));
    }
}
