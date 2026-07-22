using System.ComponentModel.DataAnnotations.Schema;

namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilityRule : EntityBase
{
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    // Rango de validez
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public RecurrenceType Recurrence { get; set; } = RecurrenceType.Weekly;

    // Para recurrencia semanal: días de la semana separados por coma (0=Sunday..6=Saturday)
    public string? DaysOfWeekCsv { get; set; }

    // Horario dentro del día
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    // Duración del slot (30 minutos)
    public TimeSpan SlotDuration { get; set; } = TimeSpan.FromMinutes(30);

    public int Capacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    // Excepciones: fechas en las que no aplica (guardamos en otra tabla si se quiere)
    // Para simplicidad almacenamos CSV de fechas ISO (yyyy-MM-dd)
    public string? ExcludedDatesCsv { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public IEnumerable<DayOfWeek> DaysOfWeek => string.IsNullOrWhiteSpace(DaysOfWeekCsv)
        ? Enumerable.Empty<DayOfWeek>()
        : DaysOfWeekCsv.Split(',').Select(s => (DayOfWeek)int.Parse(s));

    [NotMapped]
    public IEnumerable<DateOnly> ExcludedDates => string.IsNullOrWhiteSpace(ExcludedDatesCsv)
        ? Enumerable.Empty<DateOnly>()
        : ExcludedDatesCsv.Split(',').Select(s => DateOnly.Parse(s));
}
