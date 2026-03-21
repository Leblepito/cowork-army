using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CoworkArmy.Domain.DataBridge;

namespace CoworkArmy.Infrastructure.DataBridge;

/// <summary>HTTP client for leblepito.com /api/cowork/* endpoints.</summary>
public class LebLepitoClient
{
    private readonly HttpClient _http;
    private readonly ILogger<LebLepitoClient> _log;
    private readonly int _timeoutMs;

    public LebLepitoClient(HttpClient http, IConfiguration config, ILogger<LebLepitoClient> log)
    {
        _http = http;
        _log = log;
        _timeoutMs = config.GetValue("DataBridge:TimeoutMs", 5000);
    }

    public async Task<MedicalFeed> FetchMedicalAsync(CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);
            var json = await FetchJsonAsync("/api/cowork/patients", cts.Token);

            return new MedicalFeed(
                PatientsToday: GetInt(json, "today"),
                SurgeryQueue: GetInt(json, "surgeryQueue"),
                VipPipeline: GetInt(json, "vipPipeline"),
                MonthlyRevenue: GetDecimal(json, "monthlyRevenue"),
                PartnerHospitals: GetInt(json, "partnerHospitals"),
                CountriesServed: GetInt(json, "countriesServed"),
                FetchedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "LebLepito medical fetch failed, returning fallback");
            return SimulatedMedicalFeed();
        }
    }

    public async Task<HotelFeed> FetchHotelAsync(CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);

            var hotelTask = FetchJsonAsync("/api/cowork/hotel", cts.Token);
            var bookingsTask = FetchJsonAsync("/api/cowork/bookings", cts.Token);
            await Task.WhenAll(hotelTask, bookingsTask);

            var hotel = hotelTask.Result;
            var bookings = bookingsTask.Result;

            return new HotelFeed(
                OccupancyPercent: GetInt(hotel, "occupancy"),
                TotalRooms: GetInt(hotel, "totalRooms"),
                CheckInsToday: GetInt(hotel, "checkInsToday"),
                CheckOutsToday: GetInt(hotel, "checkOutsToday"),
                NewReservations: GetInt(hotel, "newReservations"),
                RevPar: GetDecimal(hotel, "revpar"),
                Tours: GetInt(bookings, "tours"),
                Transfers: GetInt(bookings, "transfers"),
                SpaBookings: GetInt(bookings, "spaBookings"),
                RestaurantReservations: GetInt(bookings, "restaurantReservations"),
                FetchedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "LebLepito hotel fetch failed, returning fallback");
            return SimulatedHotelFeed();
        }
    }

    private async Task<JsonElement> FetchJsonAsync(string path, CancellationToken ct)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonDocument.Parse(json).RootElement;
    }

    private static int GetInt(JsonElement root, string key)
        => root.TryGetProperty(key, out var el) && el.TryGetInt32(out var val) ? val : 0;

    private static decimal GetDecimal(JsonElement root, string key)
        => root.TryGetProperty(key, out var el) && el.TryGetDecimal(out var val) ? val : 0;

    private static MedicalFeed SimulatedMedicalFeed()
    {
        var rng = Random.Shared;
        return new MedicalFeed(
            PatientsToday: rng.Next(5, 20),
            SurgeryQueue: rng.Next(1, 6),
            VipPipeline: rng.Next(0, 4),
            MonthlyRevenue: rng.Next(30000, 80000),
            PartnerHospitals: 9,
            CountriesServed: 32,
            FetchedAt: DateTime.UtcNow);
    }

    private static HotelFeed SimulatedHotelFeed()
    {
        var rng = Random.Shared;
        return new HotelFeed(
            OccupancyPercent: rng.Next(60, 95),
            TotalRooms: 60,
            CheckInsToday: rng.Next(2, 10),
            CheckOutsToday: rng.Next(1, 7),
            NewReservations: rng.Next(3, 15),
            RevPar: rng.Next(70, 120),
            Tours: rng.Next(2, 8),
            Transfers: rng.Next(3, 10),
            SpaBookings: rng.Next(1, 6),
            RestaurantReservations: rng.Next(4, 15),
            FetchedAt: DateTime.UtcNow);
    }
}
