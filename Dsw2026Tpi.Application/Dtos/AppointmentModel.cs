namespace Dsw2026Tpi.Application.Dtos;

public static class AppointmentModel
{
    public record DailyResponse(
        Guid Id,
        Guid DoctorId,
        string DoctorName,
        string PatientUserId,
        string Reason,
        string Status,
        DateTime StartTime,
        DateTime EndTime);
        public record CreateRequest(Guid DoctorId, Guid AvailabilitySlotId, long PatientDni, string Reason);
        public record CreateResponse(Guid Id, string Status, DateTime StartTime, DateTime EndTime);
}