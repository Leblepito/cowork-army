using CoworkArmy.Application.DataBridge.DTOs;
using CoworkArmy.Domain.DataBridge;

namespace CoworkArmy.Application.DataBridge.Queries;

public class GetLiveFeedQueryHandler
{
    private readonly IDataBridgeService _bridge;
    public GetLiveFeedQueryHandler(IDataBridgeService bridge) => _bridge = bridge;

    public async Task<TradeFeedDto> GetTradeAsync(CancellationToken ct = default)
    {
        var f = await _bridge.GetTradeFeedAsync(ct);
        return new TradeFeedDto(f.BtcPrice, f.EthPrice, f.BtcChange24h, f.EthChange24h,
            f.OpenPositions, f.TotalPnl, f.ActiveSignals, f.FetchedAt);
    }

    public async Task<MedicalFeedDto> GetMedicalAsync(CancellationToken ct = default)
    {
        var f = await _bridge.GetMedicalFeedAsync(ct);
        return new MedicalFeedDto(f.PatientsToday, f.SurgeryQueue, f.VipPipeline,
            f.MonthlyRevenue, f.PartnerHospitals, f.CountriesServed, f.FetchedAt);
    }

    public async Task<HotelFeedDto> GetHotelAsync(CancellationToken ct = default)
    {
        var f = await _bridge.GetHotelFeedAsync(ct);
        return new HotelFeedDto(f.OccupancyPercent, f.TotalRooms, f.CheckInsToday,
            f.CheckOutsToday, f.NewReservations, f.RevPar, f.Tours, f.Transfers,
            f.SpaBookings, f.RestaurantReservations, f.FetchedAt);
    }

    public async Task<object> GetAllAsync(CancellationToken ct = default)
    {
        var tradeTask = GetTradeAsync(ct);
        var medicalTask = GetMedicalAsync(ct);
        var hotelTask = GetHotelAsync(ct);
        await Task.WhenAll(tradeTask, medicalTask, hotelTask);
        return new { trade = tradeTask.Result, medical = medicalTask.Result, hotel = hotelTask.Result };
    }
}
