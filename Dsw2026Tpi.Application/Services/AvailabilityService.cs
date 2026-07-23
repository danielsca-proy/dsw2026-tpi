using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;

    public AvailabilityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request)
    {
        // Verificar doctorId válido y que el doctor exista y este activo/no eliminado
        if (!Guid.TryParse(request.DoctorId, out var doctorId))
            throw new ValidationException().WithDetail("doctorId", "formato inválido, se requiere Guid");

        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor is null || doctor.Deleted || !doctor.IsActive)
            throw new EntityNotFoundException("Doctor");

        // Validaciones del request
        if (request.Days == null || !request.Days.Any())
            throw new ValidationException().WithDetail("days", "se requiere al menos un dia con horario");

        // Parsear startTime/endTime (strings "HH:mm") y validar. Guardar el primer horario para StartTime/EndTime de la regla.
        TimeSpan? firstStart = null;
        TimeSpan? firstEnd = null;
        var parsedDays = new System.Collections.Generic.List<int>();

        foreach (var d in request.Days)
        {
            // Parsear day: aceptar "0".."6" o nombres de día (Sunday..Saturday)
            int dayNum;
            if (!int.TryParse(d.Day, out dayNum))
            {
                if (!Enum.TryParse<System.DayOfWeek>(d.Day, true, out var dow))
                    throw new ValidationException().WithDetail($"days[{d.Day}]", "dia inválido, use 0..6 o nombre del día");
                dayNum = (int)dow;
            }

            if (dayNum < 0 || dayNum > 6)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "dia inválido, debe ser 0..6");

            if (!TimeSpan.TryParseExact(d.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
                throw new ValidationException().WithDetail($"days[{d.Day}].startTime", "formato inválido, se requiere HH:mm");

            if (!TimeSpan.TryParseExact(d.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var end))
                throw new ValidationException().WithDetail($"days[{d.Day}].endTime", "formato inválido, se requiere HH:mm");

            if (start >= end)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "La hora de inicio debe ser antes de la hora de finalización");

            parsedDays.Add(dayNum);

            if (firstStart is null)
            {
                firstStart = start;
                firstEnd = end;
            }
        }

        // Construir la regla de disponibilidad usando el primer horario parseado
        var rule = new AvailabilityRule
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            // Para Day 2 usamos valores por defecto para propiedades no incluidas en el body
            EffectiveFrom = DateTime.UtcNow,
            EffectiveTo = null,
            Recurrence = Dsw2026Tpi.Domain.Entities.RecurrenceType.Weekly,
            DaysOfWeekCsv = string.Join(',', parsedDays),
            StartTime = firstStart.GetValueOrDefault(TimeSpan.Zero),
            EndTime = firstEnd.GetValueOrDefault(TimeSpan.Zero),
            SlotDuration = TimeSpan.FromMinutes(30),
            Capacity = 1,
            IsActive = true,
            ExcludedDatesCsv = null
        };

        await _persistence.Add(rule);

        return new AvailabilityModel.Response
        {
            Id = rule.Id,
            DoctorId = rule.DoctorId,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            Recurrence = rule.Recurrence,
            DaysOfWeekCsv = rule.DaysOfWeekCsv,
            StartTime = rule.StartTime,
            EndTime = rule.EndTime,
            SlotDuration = rule.SlotDuration,
            Capacity = rule.Capacity,
            IsActive = rule.IsActive,
            ExcludedDatesCsv = rule.ExcludedDatesCsv
        };
    }
}
