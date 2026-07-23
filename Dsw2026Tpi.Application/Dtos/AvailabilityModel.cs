using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public static class AvailabilityModel
{
    public class Request
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public Dsw2026Tpi.Domain.Entities.RecurrenceType Recurrence { get; set; } = Dsw2026Tpi.Domain.Entities.RecurrenceType.Weekly;

        public string? DaysOfWeekCsv { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public TimeSpan SlotDuration { get; set; } = TimeSpan.FromMinutes(30);

        public int Capacity { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public string? ExcludedDatesCsv { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public Dsw2026Tpi.Domain.Entities.RecurrenceType Recurrence { get; set; }
        public string? DaysOfWeekCsv { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan SlotDuration { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public string? ExcludedDatesCsv { get; set; }
    }
}
