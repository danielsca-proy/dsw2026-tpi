using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;
    private const string DoctorNotFound = "Doctor";
    private const string SpecialityNotFound = "SpecialityId";

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    //metodo para obtener todos los doctores paginado y filtrado
    public async Task<Pagination<DoctorModel.Response>> GetAll( int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => !d.Deleted && d.IsActive && (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)), d => d.Name, nameof(Doctor.Speciality));
        return doctors.Map(MapResponse);
    }

    //metodo para crear un doctor
    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId);

        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException(SpecialityNotFound);

        var doctor = new Doctor( request.Name, request.LicenseNumber, speciality);
        await _persistence.Add(doctor);

        return MapResponse(doctor);
    }

    //metodo para actualizar un doctor
    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException(DoctorNotFound);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialtyId);

        if (speciality is null || speciality.Deleted)
            throw new EntityNotFoundException(SpecialityNotFound);

        doctor.UpdateDetails(request.Name, request.LicenseNumber, speciality);
        await _persistence.Update(doctor);

        return MapResponse(doctor);
    }

    //metodo para obtener la disponibilidad de un doctor por id
    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>>GetAvailabilities(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor is null || doctor.Deleted || !doctor.IsActive)
            throw new EntityNotFoundException(DoctorNotFound);

        var now = DateTime.UtcNow;

        var nextMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

        var slots = await _persistence.GetFiltered<AvailabilitySlot>(slot =>
                slot.DoctorId == id &&
                slot.Status == SlotStatus.AVAILABLE &&
                slot.Start >= now &&
                slot.Start < nextMonth);

        if (slots is null)
            return Enumerable.Empty<DoctorModel.AvailabilityResponse>();

        return slots
            .OrderBy(slot => slot.Start)
            .Select(slot => new DoctorModel.AvailabilityResponse(slot.Id, GetDayName(slot.Start.DayOfWeek), slot.Start.TimeOfDay.ToString(@"hh\:mm"), slot.End.TimeOfDay.ToString(@"hh\:mm")))
            .ToList();
    }

    //metodo para eliminar un doctor (logicamente)
    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor is null || doctor.Deleted)
            throw new EntityNotFoundException(DoctorNotFound);

        doctor.MarkAsDeleted();
        await _persistence.Update(doctor);
    }

    //metodo para tomar el nombre del dia de la semana
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

    //metodo para ordenar los dias de la semana
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

    //metodo para mapear la respuesta del doctor
    private static DoctorModel.Response MapResponse(Doctor doctor)
    {
        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber, new DoctorModel.SpecialtyDto(doctor.SpecialityId, doctor.Speciality?.Name ?? string.Empty));
    }
}