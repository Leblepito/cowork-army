namespace CoworkArmy.Domain.DataBridge;

/// <summary>Hotel department live feed — value object (immutable).</summary>
public sealed record HotelFeed(
    int OccupancyPercent,
    int TotalRooms,
    int CheckInsToday,
    int CheckOutsToday,
    int NewReservations,
    decimal RevPar,
    int Tours,
    int Transfers,
    int SpaBookings,
    int RestaurantReservations,
    DateTime FetchedAt)
{
    public static HotelFeed Empty => new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, DateTime.UtcNow);
}
