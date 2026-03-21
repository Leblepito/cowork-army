namespace CoworkArmy.Domain.DataBridge;

/// <summary>Fetches live data from external source systems.</summary>
public interface IDataBridgeService
{
    Task<TradeFeed> GetTradeFeedAsync(CancellationToken ct = default);
    Task<MedicalFeed> GetMedicalFeedAsync(CancellationToken ct = default);
    Task<HotelFeed> GetHotelFeedAsync(CancellationToken ct = default);
}
