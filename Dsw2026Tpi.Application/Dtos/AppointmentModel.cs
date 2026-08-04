namespace Dsw2026Tpi.Application.Dtos;

public static class AppointmentModel
{
    //contiene información administrativa completa
    public record AdministrativeResponse(Guid AppointmentsId, string AppointmentsStatus, AdministrativePatient Patient, AdministrativeDoctor Doctor);
    public record SearchAdministrativeResponse(Guid AppointmentsId, string AppointmentsStatus, AdministrativePatient Patient, AdministrativeDoctor Doctor, DateTime AvailableTime);
    public record AdministrativePatient(long Dni, string FullName);
    public record AdministrativeDoctor(Guid DoctorId, string Name, AdministrativeSpecialty Specialty);
    public record AdministrativeSpecialty(Guid SpecialtyId, string Name);

    //----------------

    public record PatientRequest(long Dni);
    public record CreateRequest(Guid DoctorId, Guid AvailabilitySlotId, PatientRequest Patient, string Reason);
    public record CreateResponse(Guid Id, string Status, DateTime StartTime, DateTime EndTime);
    public record GetByDateQuery(string Date, int PageSize, int PageIndex);
    public record SearchQuery(Guid? SpecialtyId, Guid? DoctorId, long? Dni, string? Date, int PageSize, int PageIndex);
    public record PatientResponse(Guid Id, Guid DoctorId, string DoctorName, string Reason, string Status, DateTime StartTime, DateTime EndTime);
}