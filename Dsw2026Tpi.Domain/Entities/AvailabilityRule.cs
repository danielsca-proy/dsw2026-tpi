using System.ComponentModel.DataAnnotations.Schema;

namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilityRule : EntityBase
{
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.WEEKLY;
    public string? DaysOfWeekCsv { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan SlotDuration { get; set; } = TimeSpan.FromMinutes(30);
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? ExcludedDatesCsv { get; set; }

    [NotMapped]
    public IEnumerable<DayOfWeek> DaysOfWeek => string.IsNullOrWhiteSpace(DaysOfWeekCsv) ? Enumerable.Empty<DayOfWeek>() : DaysOfWeekCsv.Split(',').Select(s => (DayOfWeek)int.Parse(s));

    [NotMapped]
    public IEnumerable<DateOnly> ExcludedDates => string.IsNullOrWhiteSpace(ExcludedDatesCsv) ? Enumerable.Empty<DateOnly>() : ExcludedDatesCsv.Split(',').Select(s => DateOnly.Parse(s));
}
