namespace Dsw2026Tpi.Api.Configurations;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions AdminLogin { get; set; } = new();

    public RateLimitPolicyOptions PatientLogin { get; set; } = new();

    public RateLimitPolicyOptions AppointmentCreate { get; set; } = new();

    public RateLimitPolicyOptions General { get; set; } = new();
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int QueueLimit { get; set; }
}