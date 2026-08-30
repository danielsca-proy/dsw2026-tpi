using Dsw2026Tpi.CrossCutting.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations;
public static class RateLimitPolicies
{
    public const string AdminLogin = "admin-login";
    public const string PatientLogin = "patient-login";
    public const string AppointmentCreate = "appointment-create";
    public const string General = "general";
}

public static class RateLimitingConfigurationExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitOptions = configuration
            .GetSection(RateLimitOptions.SectionName)
            .Get<RateLimitOptions>() ?? throw new InvalidOperationException($"No se encontró la sección de configuración " + $"'{RateLimitOptions.SectionName}'.");

        ValidateOptions(rateLimitOptions);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RateLimiting");

                var userName = httpContext.User.Identity?.Name ?? "anonymous";
                var ipAddress = GetClientIp(httpContext);

                logger.LogWarning("Solicitud rechazada por rate limiting. " + "Usuario: {UserName}; IP: {IpAddress}; " + "Método: {Method}; Ruta: {Path}", userName, ipAddress, httpContext.Request.Method, httpContext.Request.Path);

                var error = new ErrorResponse("RATE_LIMIT_EXCEEDED", "Se excedió el límite de solicitudes permitido.");

                var json = JsonSerializer.Serialize(error, AppJsonOptions.Default);

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                httpContext.Response.ContentType = "application/json";

                await httpContext.Response.WriteAsync(json, cancellationToken);
            };

            options.AddPolicy(RateLimitPolicies.AdminLogin, httpContext =>
                {
                    var partitionKey = $"admin-login:{GetClientIp(httpContext)}";
                    return CreatePartition(partitionKey,rateLimitOptions.AdminLogin);
                });

            options.AddPolicy(RateLimitPolicies.PatientLogin, httpContext =>
                {
                    var partitionKey = $"patient-login:{GetClientIp(httpContext)}";
                    return CreatePartition(partitionKey, rateLimitOptions.PatientLogin);
                });

            options.AddPolicy(RateLimitPolicies.AppointmentCreate, httpContext =>
                {
                    var partitionKey = $"appointment-create:{GetUserOrIp(httpContext)}";
                    return CreatePartition(partitionKey, rateLimitOptions.AppointmentCreate);
                });

            options.AddPolicy(RateLimitPolicies.General, httpContext =>
                {
                    var partitionKey = $"general:{GetUserOrIp(httpContext)}";
                    return CreatePartition(partitionKey, rateLimitOptions.General);
                });
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePartition(string partitionKey, RateLimitPolicyOptions policy)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                QueueLimit = policy.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    }

    private static string GetUserOrIp(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(context.User.Identity.Name))
            return $"user:{context.User.Identity.Name}";

        return $"ip:{GetClientIp(context)}";
    }

    private static string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static void ValidateOptions(
        RateLimitOptions options)
    {
        ValidatePolicy(nameof(options.AdminLogin), options.AdminLogin);
        ValidatePolicy(nameof(options.PatientLogin), options.PatientLogin);
        ValidatePolicy(nameof(options.AppointmentCreate), options.AppointmentCreate);
        ValidatePolicy(nameof(options.General), options.General);
    }

    private static void ValidatePolicy(string policyName, RateLimitPolicyOptions policy)
    {
        if (policy.PermitLimit <= 0)
            throw new InvalidOperationException($"RateLimiting:{policyName}:PermitLimit " + "debe ser mayor que cero.");

        if (policy.WindowSeconds <= 0)
            throw new InvalidOperationException($"RateLimiting:{policyName}:WindowSeconds " + "debe ser mayor que cero.");

        if (policy.QueueLimit != 0)
            throw new InvalidOperationException($"RateLimiting:{policyName}:QueueLimit " + "debe ser cero porque el enunciado no permite " + "encolar solicitudes rechazadas.");
    }
}
