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

    public AvailabilityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    private async Task GenerateSlotsForRule(AvailabilityRule rule, DateTime fromDate, DateTime toDate)
    {
        // Convertir CSV de feriados a conjunto de DateOnly
        var excluded = new HashSet<DateOnly>();
        if (!string.IsNullOrWhiteSpace(rule.ExcludedDatesCsv))
        {
            foreach (var s in rule.ExcludedDatesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (DateOnly.TryParse(s, out var d)) excluded.Add(d);
            }
        }

        var daysOfWeek = rule.DaysOfWeekCsv?.Split(',').Select(int.Parse).ToHashSet() ?? new HashSet<int>();

        var current = fromDate.Date;
        var slotsToAdd = new List<AvailabilitySlot>();

        while (current.Date <= toDate.Date)
        {
            if (!daysOfWeek.Contains((int)current.DayOfWeek))
            {
                current = current.AddDays(1);
                continue;
            }

            var dateOnly = DateOnly.FromDateTime(current);
            if (excluded.Contains(dateOnly))
            {
                current = current.AddDays(1);
                continue;
            }

            // generar slots entre StartTime y EndTime
            var startTime = rule.StartTime;
            var endTime = rule.EndTime;

            var slotStart = current.Add(startTime);
            var slotDuration = rule.SlotDuration;

            while (slotStart.Add(slotDuration) <= current.Add(endTime))
            {
                var slotEnd = slotStart.Add(slotDuration);

                // crear slot en UTC (se asume que slotStart ya está en UTC)
                var slot = new AvailabilitySlot
                {
                    Id = Guid.NewGuid(),
                    DoctorId = rule.DoctorId,
                    RuleId = rule.Id,
                    Start = DateTime.SpecifyKind(slotStart, DateTimeKind.Utc),
                    End = DateTime.SpecifyKind(slotEnd, DateTimeKind.Utc),
                    Status = SlotStatus.Available,
                    Capacity = rule.Capacity,
                    BookedCount = 0
                };

                slotsToAdd.Add(slot);

                slotStart = slotStart.Add(slotDuration);
            }

            current = current.AddDays(1);
        }

        // Persistir slots en batch
        foreach (var s in slotsToAdd)
        {
            await _persistence.Add(s);
        }
    }

   public async Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request)
{
    if (!Guid.TryParse(request.DoctorId, out var doctorId))
        throw new ValidationException().WithDetail("doctorId", "formato inválido, se requiere Guid");

    var doctor = await _persistence.GetById<Doctor>(doctorId);
    if (doctor is null || doctor.Deleted || !doctor.IsActive)
        throw new EntityNotFoundException("Doctor");

    if (request.Days == null || !request.Days.Any())
        throw new ValidationException().WithDetail("days", "se requiere al menos un dia con horario");

    // Parsear cada día individualmente, sin perder su horario propio
    var parsed = new List<(int Day, TimeSpan Start, TimeSpan End)>();

    foreach (var d in request.Days)
    {
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

        parsed.Add((dayNum, start, end));
    }

    // Verificar solapamientos con reglas existentes del mismo doctor, por cada día parseado
    var existing = await _persistence.GetFiltered<AvailabilityRule>(r => r.DoctorId == doctorId);
    foreach (var (day, start, end) in parsed)
    {
        foreach (var er in existing ?? Enumerable.Empty<AvailabilityRule>())
        {
            var existingDays = er.DaysOfWeekCsv?.Split(',').Select(int.Parse).ToHashSet() ?? new HashSet<int>();
            if (!existingDays.Contains(day)) continue;

            if (er.StartTime < end && er.EndTime > start)
                throw new BusinessRuleException("La regla de disponibilidad solapa con una regla existente.", "AVAILABILITY_OVERLAP");
        }
    }

    // Agrupar los días que comparten el mismo horario → una regla por grupo
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
            Recurrence = RecurrenceType.Weekly,
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

    // Devolvemos la primera regla creada como referencia (Response solo soporta una)
    var main = createdRules.First();
    return new AvailabilityModel.Response
    {
        Id = main.Id,
        DoctorId = main.DoctorId,
        EffectiveFrom = main.EffectiveFrom,
        EffectiveTo = main.EffectiveTo,
        Recurrence = main.Recurrence,
        DaysOfWeekCsv = string.Join(';', createdRules.Select(r => r.DaysOfWeekCsv)),
        StartTime = main.StartTime,
        EndTime = main.EndTime,
        SlotDuration = main.SlotDuration,
        Capacity = main.Capacity,
        IsActive = main.IsActive,
        ExcludedDatesCsv = main.ExcludedDatesCsv
    };
}

    public async Task<AvailabilityModel.Response> Update(AvailabilityModel.Request request)
    {
        if (!Guid.TryParse(request.DoctorId, out var doctorId))
            throw new ValidationException().WithDetail("doctorId", "formato inválido, se requiere Guid");

        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor is null || doctor.Deleted || !doctor.IsActive)
            throw new EntityNotFoundException("Doctor");

        if (request.Days == null || !request.Days.Any())
            throw new ValidationException().WithDetail("days", "se requiere al menos un dia con horario");

        // Parsear cada día individualmente, sin perder su horario propio
        var parsed = new List<(int Day, TimeSpan Start, TimeSpan End)>();

        foreach (var d in request.Days)
        {
            int dayNum;
            if (!int.TryParse(d.Day, out dayNum))
            {
                if (!Enum.TryParse<System.DayOfWeek>(d.Day, true, out var dow))
                    throw new ValidationException().WithDetail($"days[{d.Day}]", "dia invalido, use 0..6 o nombre del día");
                dayNum = (int)dow;
            }

            if (dayNum < 0 || dayNum > 6)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "dia invalido, debe ser 0..6");

            if (!TimeSpan.TryParseExact(d.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var start))
                throw new ValidationException().WithDetail($"days[{d.Day}].startTime", "formato invalido, se requiere HH:mm");

            if (!TimeSpan.TryParseExact(d.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var end))
                throw new ValidationException().WithDetail($"days[{d.Day}].endTime", "formato invalido, se requiere HH:mm");

            if (start >= end)
                throw new ValidationException().WithDetail($"days[{d.Day}]", "La hora de inicio debe ser antes de la hora de finalización");

            parsed.Add((dayNum, start, end));
        }

        // Eliminar reglas existentes del médico que sean efectivas en el mes actual (aproximación simple)
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        var existingRules = await _persistence.GetFiltered<AvailabilityRule>(
            r => r.DoctorId == doctorId && r.EffectiveFrom >= monthStart && r.EffectiveFrom <= monthEnd);

        if (existingRules != null)
        {
            foreach (var er in existingRules)
            {
                // No hay borrado lógico en AvailabilityRule, por eso las eliminamos físicamente
                await _persistence.Delete(er);
            }
        }

        // Eliminar slots existentes del doctor dentro del rango del mes
        var monthStartDate = DateTime.UtcNow.Date;
        var monthEndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
            DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));

        var existingSlots = await _persistence.GetFiltered<AvailabilitySlot>(
            s => s.DoctorId == doctorId && s.Start >= monthStartDate && s.Start <= monthEndDate);

        if (existingSlots != null)
        {
            foreach (var es in existingSlots)
            {
                await _persistence.Delete(es);
            }
        }

        // Agrupar los días que comparten el mismo horario → una regla por grupo
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
                Recurrence = RecurrenceType.Weekly,
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

        var main = createdRules.First();
        return new AvailabilityModel.Response
        {
            Id = main.Id,
            DoctorId = main.DoctorId,
            EffectiveFrom = main.EffectiveFrom,
            EffectiveTo = main.EffectiveTo,
            Recurrence = main.Recurrence,
            DaysOfWeekCsv = string.Join(';', createdRules.Select(r => r.DaysOfWeekCsv)),
            StartTime = main.StartTime,
            EndTime = main.EndTime,
            SlotDuration = main.SlotDuration,
            Capacity = main.Capacity,
            IsActive = main.IsActive,
            ExcludedDatesCsv = main.ExcludedDatesCsv
        };
    }
}
