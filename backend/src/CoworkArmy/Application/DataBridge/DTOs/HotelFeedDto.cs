namespace CoworkArmy.Application.DataBridge.DTOs;

public record HotelFeedDto(
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
    DateTime FetchedAt);
