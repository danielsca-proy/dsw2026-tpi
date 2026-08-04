using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System.Collections.Generic;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;
    private readonly IHolidayProvider _holidayProvider;

    public AvailabilityService(IPersistence persistence, IHolidayProvider holidayProvider)
    {
        _persistence = persistence;
        _holidayProvider = holidayProvider;
    }

    private static readonly string[] ValidDayNames =
    {
        "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO"
    };

    //metodo para normalizar el nombre del dia
    private static string NormalizeDayName(string day)
    {
        if (string.IsNullOrWhiteSpace(day))
            throw new ValidationException().WithDetail("days", "día inválido");

        var normalized = day.Trim()
            .ToUpperInvariant()
            .Replace("É", "E").Replace("Á", "A");

        if (!ValidDayNames.Contains(normalized))
            throw new ValidationException().WithDetail("days", $"día inválido: '{day}'. Debe ser LUNES, MARTES, MIERCOLES, JUEVES, VIERNES, SABADO o DOMINGO");

        return normalized;
    }

    private static void ValidateNoOverlapsWithinRequest(IReadOnlyCollection<(string Day, TimeSpan Start, TimeSpan End)> schedules)
    {
        var schedulesByDay = schedules.GroupBy(schedule => schedule.Day);

        foreach (var dayGroup in schedulesByDay)
        {
            var orderedSchedules = dayGroup
                .OrderBy(schedule => schedule.Start)
                .ThenBy(schedule => schedule.End)
                .ToList();

            for (var i = 1; i < orderedSchedules.Count; i++)
            {
                var previous = orderedSchedules[i - 1];
                var current = orderedSchedules[i];

                if (current.Start < previous.End)
                {
                    var previousRange = $"{previous.Start:hh\\:mm}-{previous.End:hh\\:mm}";

                    var currentRange = $"{current.Start:hh\\:mm}-{current.End:hh\\:mm}";

                    throw new ValidationException().WithDetail("days", $"Los horarios del día {dayGroup.Key} se solapan: " + $"{previousRange} y {currentRange}");
                }
            }
        }
    }

    //metodo para obtener el nombre del dia
    private static string GetDayName(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Monday => "LUNES",
        DayOfWeek.Tuesday => "MARTES",
        DayOfWeek.Wednesday => "MIERCOLES",
        DayOfWeek.Thursday => "JUEVES",
        DayOfWeek.Friday => "VIERNES",
        DayOfWeek.Saturday => "SABADO",
        DayOfWeek.Sunday => "DOMINGO",
        _ => throw new ArgumentOutOfRangeException(nameof(dow))
    };

    //metodo para generar los slots de disponibilidad a partir de una regla
    private async Task GenerateSlotsForRule(AvailabilityRule rule, DateTime fromDate, DateTime toDate)
    {
        var excluded = new HashSet<DateOnly>();
        if (!string.IsNullOrWhiteSpace(rule.ExcludedDatesCsv))
        {
            foreach (var s in rule.ExcludedDatesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (DateOnly.TryParse(s, out var d)) excluded.Add(d);
            }
        }

        var daysOfWeek = rule.DaysOfWeekCsv?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToUpperInvariant())
            .ToHashSet() ?? new HashSet<string>();

        var current = fromDate.Date;
        var slotsToAdd = new List<AvailabilitySlot>();

        while (current.Date <= toDate.Date)
        {
            if (!daysOfWeek.Contains(GetDayName(current.DayOfWeek)))
            {
                current = current.AddDays(1);
                continue;
            }

            var dateOnly = DateOnly.FromDateTime(current);
            if (excluded.Contains(dateOnly) || _holidayProvider.IsNonWorkingDay(dateOnly))
            {
                current = current.AddDays(1);
                continue;
            }

            var startTime = rule.StartTime;
            var endTime = rule.EndTime;

            var slotStart = current.Add(startTime);
            var slotDuration = rule.SlotDuration;

            while (slotStart.Add(slotDuration) <= current.Add(endTime))
            {
                var slotEnd = slotStart.Add(slotDuration);
                var slot = new AvailabilitySlot
                {
                    Id = Guid.NewGuid(),
                    DoctorId = rule.DoctorId,
                    RuleId = rule.Id,
                    Start = DateTime.SpecifyKind(slotStart, DateTimeKind.Utc),
                    End = DateTime.SpecifyKind(slotEnd, DateTimeKind.Utc),
                    Status = SlotStatus.AVAILABLE,
                    Capacity = rule.Capacity,
                    BookedCount = 0
                };

                slotsToAdd.Add(slot);

                slotStart = slotStart.Add(slotDuration);
            }

            current = current.AddDays(1);
        }

        foreach (var s in slotsToAdd)
        {
            await _persistence.Add(s);
        }
    }

    //metodo para crear intervalos de disponibilidad para un doctor
    public async Task<List<AvailabilityModel.Response>> Create(AvailabilityModel.Request request)
    {
        var doctorId = Guid.Parse(request.DoctorId);
        var doctor = await _persistence.GetById<Doctor>(doctorId);

        if (doctor is null || doctor.Deleted || !doctor.IsActive)
            throw new EntityNotFoundException("Doctor");

        if (request.Days == null || !request.Days.Any())
            throw new ValidationException().WithDetail("days", "se requiere al menos un dia con horario");

        var parsed = new List<(string Day, TimeSpan Start, TimeSpan End)>();

        foreach (var d in request.Days)
        {
            var dayName = NormalizeDayName(d.Day);
            if (!TimeSpan.TryParseExact(d.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
                throw new ValidationException().WithDetail($"days[{d.Day}].startTime", "Formato de hora inválido. Debe ser HH:mm");


            if (!TimeSpan.TryParseExact(d.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var end))
                throw new ValidationException().WithDetail($"days[{d.Day}].endTime", "Formato de hora inválido. Debe ser HH:mm");

            if (start >= end)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "La hora de inicio debe ser antes de la hora de finalización");

            parsed.Add((dayName, start, end));
        }

        ValidateNoOverlapsWithinRequest(parsed);

        var existing = await _persistence.GetFiltered<AvailabilityRule>(r => r.DoctorId == doctorId);
        foreach (var (day, start, end) in parsed)
        {
            foreach (var er in existing ?? Enumerable.Empty<AvailabilityRule>())
            {
                var existingDays = er.DaysOfWeekCsv?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToUpperInvariant())
                    .ToHashSet() ?? new HashSet<string>();

                if (!existingDays.Contains(day)) continue;

                if (er.StartTime < end && er.EndTime > start)
                    throw new BusinessRuleException("La regla de disponibilidad solapa con una regla existente.", "AVAILABILITY_OVERLAP");
            }
        }

        var groups = parsed.GroupBy(p => (p.Start, p.End));
        var createdRules = new List<AvailabilityRule>();

        foreach (var group in groups)
        {
            var rule = new AvailabilityRule
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
                Recurrence = RecurrenceType.WEEKLY,
                DaysOfWeekCsv = string.Join(',', group.Select(g => g.Day)),
                StartTime = group.Key.Start,
                EndTime = group.Key.End,
                SlotDuration = TimeSpan.FromMinutes(30),
                Capacity = 1,
                IsActive = true,
                ExcludedDatesCsv = null
            };

            await _persistence.Add(rule);
            await GenerateSlotsForRule(rule, DateTime.UtcNow.Date,
                new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month)));

            createdRules.Add(rule);
        }

        return createdRules.Select(r => new AvailabilityModel.Response
        {
            Id = r.Id,
            DoctorId = r.DoctorId,
            EffectiveFrom = r.EffectiveFrom,
            EffectiveTo = r.EffectiveTo,
            Recurrence = r.Recurrence,
            DaysOfWeekCsv = r.DaysOfWeekCsv,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            SlotDuration = r.SlotDuration,
            Capacity = r.Capacity,
            IsActive = r.IsActive,
            ExcludedDatesCsv = r.ExcludedDatesCsv
        }).ToList();
    }

    //metodo para actualizar intervalos de disponibilidad para un doctor
    public async Task<List<AvailabilityModel.Response>> Update(AvailabilityModel.Request request)
    {
        var doctorId = Guid.Parse(request.DoctorId);
        var doctor = await _persistence.GetById<Doctor>(doctorId);

        if (doctor is null || doctor.Deleted || !doctor.IsActive)
            throw new EntityNotFoundException("Doctor");

        if (request.Days == null || !request.Days.Any())
            throw new ValidationException().WithDetail("days", "se requiere al menos un dia con horario");

        var parsed = new List<(string Day, TimeSpan Start, TimeSpan End)>();

        foreach (var d in request.Days)
        {
            var dayName = NormalizeDayName(d.Day);

            if (!TimeSpan.TryParseExact(d.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
                throw new ValidationException().WithDetail($"days[{d.Day}].startTime", "formato inválido, se requiere HH:mm");

            if (!TimeSpan.TryParseExact(d.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var end))
                throw new ValidationException().WithDetail($"days[{d.Day}].endTime", "formato inválido, se requiere HH:mm");

            if (start >= end)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "La hora de inicio debe ser antes de la hora de finalización");

            parsed.Add((dayName, start, end));
        }

        ValidateNoOverlapsWithinRequest(parsed);

        var monthStartDate = DateTime.UtcNow.Date;
        var monthEndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));

        var slotsInMonth = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == doctorId && s.Start >= monthStartDate && s.Start <= monthEndDate);

        if (slotsInMonth != null && slotsInMonth.Any(s => s.Status == SlotStatus.BOOKED))
            throw new BusinessRuleException("No se puede actualizar la disponibilidad: existen turnos reservados en el mes actual.", "AVAILABILITY_HAS_BOOKED_SLOTS");

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var existingRules = await _persistence.GetFiltered<AvailabilityRule>(
            r => r.DoctorId == doctorId && r.EffectiveFrom >= monthStart && r.EffectiveFrom <= monthEnd);

        if (existingRules != null)
        {
            foreach (var er in existingRules)
            {
                await _persistence.Delete(er);
            }
        }

        if (slotsInMonth != null)
        {
            foreach (var es in slotsInMonth)
            {
                await _persistence.Delete(es);
            }
        }

        var groups = parsed.GroupBy(p => (p.Start, p.End));
        var createdRules = new List<AvailabilityRule>();

        foreach (var group in groups)
        {
            var rule = new AvailabilityRule
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
                Recurrence = RecurrenceType.WEEKLY,
                DaysOfWeekCsv = string.Join(',', group.Select(g => g.Day)),
                StartTime = group.Key.Start,
                EndTime = group.Key.End,
                SlotDuration = TimeSpan.FromMinutes(30),
                Capacity = 1,
                IsActive = true,
                ExcludedDatesCsv = null
            };

            await _persistence.Add(rule);
            await GenerateSlotsForRule(rule, monthStartDate, monthEndDate);

            createdRules.Add(rule);
        }

        return createdRules.Select(r => new AvailabilityModel.Response
        {
            Id = r.Id,
            DoctorId = r.DoctorId,
            EffectiveFrom = r.EffectiveFrom,
            EffectiveTo = r.EffectiveTo,
            Recurrence = r.Recurrence,
            DaysOfWeekCsv = r.DaysOfWeekCsv,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            SlotDuration = r.SlotDuration,
            Capacity = r.Capacity,
            IsActive = r.IsActive,
            ExcludedDatesCsv = r.ExcludedDatesCsv
        }).ToList();
    }
}