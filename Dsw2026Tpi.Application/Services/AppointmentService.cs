using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentService(IPersistence persistence, UserManager<ApplicationUser> userManager)
    {
        _persistence = persistence;
        _userManager = userManager;
    }

    public async Task<AppointmentModel.CreateResponse> Create(AppointmentModel.CreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
            throw new ValidationException().WithDetail("reason", "obligatorio, mínimo 5 caracteres");

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor not found");

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId);
        if (slot is null || slot.DoctorId != request.DoctorId)
            throw new EntityNotFoundException("AvailabilitySlot not found");

        if (slot.Status != SlotStatus.Available)
            throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya no está disponible");

        if (slot.Start <= DateTime.UtcNow)
            throw new ValidationException().WithDetail("availabilitySlotId", "no se pueden reservar turnos pasados");

        var patient = _userManager.Users.FirstOrDefault(u => u.Dni == request.PatientDni);
        if (patient is null)
            throw new EntityNotFoundException("Patient not found");

        slot.Status = SlotStatus.Booked;
        slot.BookedCount++;

        try
        {
            await _persistence.Update(slot);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("APPOINTMENT_CONFLICT", "El turno ya fue reservado por otro paciente, elegí otro horario");
        }

        var appointment = new Appointment(slot, patient.Id, request.Reason);
        await _persistence.Add(appointment);

        return new AppointmentModel.CreateResponse(
            appointment.Id,
            appointment.Status.ToString(),
            slot.Start,
            slot.End);
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