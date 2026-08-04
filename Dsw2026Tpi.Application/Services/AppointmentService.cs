using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
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

    //metodo para la creacion de un turno
    public async Task<AppointmentModel.CreateResponse> Create(AppointmentModel.CreateRequest request, string authenticatedUserName)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor");

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilitySlotId);
        if (slot is null || slot.DoctorId != request.DoctorId)
            throw new EntityNotFoundException("AvailabilitySlot");

        if (slot.Start <= DateTime.UtcNow)
            throw new ValidationException().WithDetail("availabilitySlotId", "no se pueden reservar turnos pasados");

        if (slot.Status != SlotStatus.Available)
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), "El turno ya no está disponible");

        var patient = await GetAuthenticatedPatient(authenticatedUserName, request.Patient!.Dni);

        slot.Status = SlotStatus.Booked;
        slot.BookedCount++;

        try
        {
            await _persistence.Update(slot);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), "El turno ya fue reservado por otro paciente, elegí otro horario");
        }

        var appointment = new Appointment(slot, patient.Id, request.Reason);
        await _persistence.Add(appointment);
        return new AppointmentModel.CreateResponse(appointment.Id, appointment.Status.ToString(), slot.Start, slot.End);
    }

    //metodo para cancelar un turno
    public async Task Cancel(Guid id, string authenticatedUserName)
    {
        var appointment = await _persistence.GetById<Appointment>(id, nameof(Appointment.AvailabilitySlot));

        if (appointment is null)
            throw new EntityNotFoundException("Appointment");

        var patient = await _userManager.FindByNameAsync(
            authenticatedUserName);

        if (patient is null)
            throw new AuthenticationException();

        if (appointment.PatientUserId != patient.Id)
            throw new EntityNotFoundException("Appointment");

        if (appointment.Status != AppointmentStatus.Booked)
        {
            throw new ConflictException(
                nameof(ErrorCodes.APPOINTMENT_CONFLICT),
                "Solo se pueden cancelar turnos reservados");
        }

        appointment.Cancel();

        if (appointment.AvailabilitySlot is not null)
        {
            appointment.AvailabilitySlot.Status = SlotStatus.Available;
            appointment.AvailabilitySlot.BookedCount = Math.Max(0, appointment.AvailabilitySlot.BookedCount - 1);

            await _persistence.Update(appointment.AvailabilitySlot);
        }

        await _persistence.Update(appointment);
    }

    //metodo para obtener los turnos de un paciente mediante su dni
    public async Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatient(long dni, string authenticatedUserName)
    {
        var patient = await GetAuthenticatedPatient(authenticatedUserName, dni);

        var appointments = await _persistence.GetFiltered<Appointment>(a => a.PatientUserId == patient.Id && a.Status == AppointmentStatus.Booked, nameof(Appointment.AvailabilitySlot), $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}");

        return (appointments ?? Enumerable.Empty<Appointment>()).Select(a => new AppointmentModel.PatientResponse(
            a.Id,
            a.AvailabilitySlot!.DoctorId,
            a.AvailabilitySlot.Doctor?.Name ?? string.Empty,
            a.Reason,
            a.Status.ToString(),
            a.AvailabilitySlot.Start,
            a.AvailabilitySlot.End
        ));
    }

    //metodo para obtener los turnos de un dia especifico
    public async Task<IEnumerable<AppointmentModel.DailyResponse>> GetByDate(DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        var appointments = await _persistence.GetFiltered<Appointment>(a => a.AvailabilitySlot!.Start >= dayStart && a.AvailabilitySlot!.Start <= dayEnd, nameof(Appointment.AvailabilitySlot), $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}");

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

    //metodo de busqueda de turnos filtrado
    public async Task<Pagination<AppointmentModel.SearchResponse>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date, int pageSize, int pageIndex)
    {
        string? patientId = null;
        if (dni.HasValue)
        {
            patientId = _userManager.Users.FirstOrDefault(u => u.Dni == dni.Value)?.Id;
            if (patientId is null)
                return Pagination<AppointmentModel.SearchResponse>.Empty;
        }

        DateTime? dayStart = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue) : null;
        DateTime? dayEnd = date.HasValue ? date.Value.ToDateTime(TimeOnly.MaxValue) : null;

        var result = await _persistence.Paginate<Appointment, DateTime>(
            pageSize, pageIndex, 
            a =>
                (!specialtyId.HasValue || a.AvailabilitySlot!.Doctor!.SpecialityId == specialtyId) &&
                (!doctorId.HasValue || a.AvailabilitySlot!.DoctorId == doctorId) &&
                (patientId == null || a.PatientUserId == patientId) &&
                (!dayStart.HasValue || (a.AvailabilitySlot!.Start >= dayStart && a.AvailabilitySlot!.Start <= dayEnd)),
            a => a.AvailabilitySlot!.Start,
            nameof(Appointment.AvailabilitySlot),
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}",
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}.{nameof(Doctor.Speciality)}");

        return result.Map(a => new AppointmentModel.SearchResponse(
            a.Id,
            new AppointmentModel.SearchSpecialty(a.AvailabilitySlot!.Doctor!.SpecialityId, a.AvailabilitySlot.Doctor.Speciality?.Name ?? string.Empty),
            new AppointmentModel.SearchDoctor(a.AvailabilitySlot.DoctorId, a.AvailabilitySlot.Doctor.Name),
            a.AvailabilitySlot.Start,
            a.Status.ToString()));
    }

    private async Task<ApplicationUser> GetAuthenticatedPatient(string authenticatedUserName, long requestedDni)
    {
        var patient = await _userManager.FindByNameAsync(authenticatedUserName);

        if (patient is null || patient.Dni != requestedDni)
            throw new AuthenticationException();

        return patient;
    }
}