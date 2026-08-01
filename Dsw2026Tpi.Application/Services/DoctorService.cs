using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => !d.Deleted &&
                 (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)),
            d => d.Name,
            nameof(Doctor.Speciality));

        return doctors.Map(MapResponse);
    }

    public async Task<DoctorModel.Response> Create(
        DoctorModel.Request request)
    {

        var speciality =
            await _persistence.GetById<Speciality>(request.SpecialityId);

        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException("Speciality not found");

        var doctor = new Doctor(
            request.Name,
            request.LicenseNumber,
            speciality);

        await _persistence.Add(doctor);

        return MapResponse(doctor);
    }

    public async Task<DoctorModel.Response> Update(
        Guid id,
        DoctorModel.Request request)
    {

        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor not found");

        var speciality =
            await _persistence.GetById<Speciality>(request.SpecialityId);

        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException("Speciality not found");

        doctor.UpdateDetails(
            request.Name,
            request.LicenseNumber,
            speciality);

        await _persistence.Update(doctor);

        return MapResponse(doctor);
    }

    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>>
    GetAvailabilities(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException("Doctor not found");

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var nextMonth = monthStart.AddMonths(1);

        var slots = await _persistence.GetFiltered<AvailabilitySlot>(
            slot => slot.DoctorId == id &&
                    slot.Start >= monthStart &&
                    slot.Start < nextMonth);

        if (slots is null)
            return Enumerable.Empty<DoctorModel.AvailabilityResponse>();

        return slots
            .GroupBy(slot => new
            {
                slot.RuleId,
                slot.Start.DayOfWeek
            })
            .Select(group => new DoctorModel.AvailabilityResponse(
                GetDayName(group.Key.DayOfWeek),
                group.Min(slot => slot.Start.TimeOfDay)
                    .ToString(@"hh\:mm"),
                group.Max(slot => slot.End.TimeOfDay)
                    .ToString(@"hh\:mm")))
            .OrderBy(item => GetDayOrder(item.Day))
            .ToList();
    }
    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || doctor.Deleted)
        {
            throw new EntityNotFoundException("Doctor not found");
        }

        doctor.Eliminar();

        await _persistence.Update(doctor);
    }

    private static string GetDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "LUNES",
            DayOfWeek.Tuesday => "MARTES",
            DayOfWeek.Wednesday => "MIÉRCOLES",
            DayOfWeek.Thursday => "JUEVES",
            DayOfWeek.Friday => "VIERNES",
            DayOfWeek.Saturday => "SÁBADO",
            DayOfWeek.Sunday => "DOMINGO",
            _ => string.Empty
        };
    }

    private static int GetDayOrder(string day)
    {
        return day switch
        {
            "LUNES" => 1,
            "MARTES" => 2,
            "MIÉRCOLES" => 3,
            "JUEVES" => 4,
            "VIERNES" => 5,
            "SÁBADO" => 6,
            "DOMINGO" => 7,
            _ => 8
        };
    }
    private static DoctorModel.Response MapResponse(Doctor doctor)
    {
        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(
                doctor.SpecialityId,
                doctor.Speciality?.Name));
    }
}