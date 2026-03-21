namespace CoworkArmy.Application.DataBridge.DTOs;

public record MedicalFeedDto(
    int PatientsToday,
    int SurgeryQueue,
    int VipPipeline,
    decimal MonthlyRevenue,
    int PartnerHospitals,
    int CountriesServed,
    DateTime FetchedAt);
