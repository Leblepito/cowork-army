namespace CoworkArmy.Domain.DataBridge;

/// <summary>Medical department live feed — value object (immutable).</summary>
public sealed record MedicalFeed(
    int PatientsToday,
    int SurgeryQueue,
    int VipPipeline,
    decimal MonthlyRevenue,
    int PartnerHospitals,
    int CountriesServed,
    DateTime FetchedAt)
{
    public static MedicalFeed Empty => new(0, 0, 0, 0, 0, 0, DateTime.UtcNow);
}
