using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Application.Dtos;

public static class AvailabilityModel
{
    public class DayRequest
    {
        // 0 = Domingo .. 6 = Sabado
        [Required]
        [JsonPropertyName("day")]
        [DefaultValue("string")]
        public string Day { get; set; } = "string";

        [Required]
        [JsonPropertyName("startTime")]
        [DefaultValue("HH:mm")]
        public string StartTime { get; set; } = "HH:mm";

        [Required]
        [JsonPropertyName("endTime")]
        [DefaultValue("HH:mm")]
        public string EndTime { get; set; } = "HH:mm";
    }

    public class Request
    {
        [Required]
        [JsonPropertyName("doctorId")]
        [DefaultValue("Guid")]
        public string DoctorId { get; set; } = "Guid";
        [Required]
        [JsonPropertyName("days")]
        public IEnumerable<DayRequest> Days { get; set; } = Enumerable.Empty<DayRequest>();
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
