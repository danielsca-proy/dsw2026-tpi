namespace Dsw2026Tpi.Application.Dtos;

public static class AppointmentModel
{
    public record DailyResponse(Guid Id, Guid DoctorId, string DoctorName, string PatientUserId, string Reason, string Status, DateTime StartTime, DateTime EndTime);
    public record PatientRequest(long Dni);
    public record CreateRequest(Guid DoctorId, Guid AvailabilitySlotId, PatientRequest Patient, string Reason);
    public record CreateResponse(Guid Id, string Status, DateTime StartTime, DateTime EndTime);
    public record SearchResponse(Guid Id, SearchSpecialty Specialty, SearchDoctor Doctor, DateTime AvailableTime, string Status);
    public record SearchSpecialty(Guid Id, string Name);
    public record SearchDoctor(Guid Id, string Name);
    public record GetByDateQuery(string Date, int PageSize, int PageIndex);
    public record SearchQuery(Guid? SpecialtyId, Guid? DoctorId, long? Dni, string? Date, int PageSize, int PageIndex);
    public record PatientResponse(Guid Id, Guid DoctorId, string DoctorName, string Reason, string Status, DateTime StartTime, DateTime EndTime);
}