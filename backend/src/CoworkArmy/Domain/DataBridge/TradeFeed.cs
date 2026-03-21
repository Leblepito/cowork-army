namespace CoworkArmy.Domain.DataBridge;

/// <summary>Trade department live feed — value object (immutable).</summary>
public sealed record TradeFeed(
    decimal BtcPrice,
    decimal EthPrice,
    decimal BtcChange24h,
    decimal EthChange24h,
    int OpenPositions,
    decimal TotalPnl,
    int ActiveSignals,
    DateTime FetchedAt)
{
    public static TradeFeed Empty => new(0, 0, 0, 0, 0, 0, 0, DateTime.UtcNow);
}
