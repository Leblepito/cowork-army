namespace CoworkArmy.Application.DataBridge.DTOs;

public record TradeFeedDto(
    decimal BtcPrice,
    decimal EthPrice,
    decimal BtcChange24h,
    decimal EthChange24h,
    int OpenPositions,
    decimal TotalPnl,
    int ActiveSignals,
    DateTime FetchedAt);
