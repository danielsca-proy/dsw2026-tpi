using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<IEnumerable<AppointmentModel.DailyResponse>> GetByDate(DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.AvailabilitySlot!.Start >= dayStart && a.AvailabilitySlot!.Start <= dayEnd,
            nameof(Appointment.AvailabilitySlot),
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}");

        return (appointments ?? Enumerable.Empty<Appointment>()).Select(a => new AppointmentModel.DailyResponse(
            a.Id,
            a.AvailabilitySlot!.DoctorId,
            a.AvailabilitySlot.Doctor?.Name ?? string.Empty,
            a.PatientUserId,
            a.Reason,
            a.Status.ToString(),
            a.AvailabilitySlot.Start,
            a.AvailabilitySlot.End
        ));
    }
}