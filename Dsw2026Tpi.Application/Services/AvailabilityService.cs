using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly Dsw2026TpiDbContext _db;

    public AvailabilityService(Dsw2026TpiDbContext db)
    {
        _db = db;
    }

    public async Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request)
    {
        // Valida que el doctor exista
        var doctor = await _db.Set<Doctor>().FindAsync(request.DoctorId);
        if (doctor == null)
            throw new Dsw2026Tpi.CrossCutting.Exceptions.ConflictException(
                Dsw2026Tpi.CrossCutting.Resources.ErrorCodes.ENTITY_NOTFOUND,
                string.Format(Dsw2026Tpi.CrossCutting.Resources.ErrorCodes.ENTITY_NOTFOUND, "Doctor"));

        var rule = new AvailabilityRule
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Recurrence = request.Recurrence,
            DaysOfWeekCsv = request.DaysOfWeekCsv,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDuration = request.SlotDuration,
            Capacity = request.Capacity,
            IsActive = request.IsActive,
            ExcludedDatesCsv = request.ExcludedDatesCsv
        };

        _db.AvailabilityRules.Add(rule);
        await _db.SaveChangesAsync();

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
