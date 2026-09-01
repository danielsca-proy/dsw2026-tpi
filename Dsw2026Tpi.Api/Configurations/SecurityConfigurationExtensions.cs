using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace Dsw2026Tpi.Api.Configurations;

public static class SecurityConfigurationExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? throw new InvalidOperationException($"No se encontró la sección de configuración '{JwtOptions.SectionName}'.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key)) throw new ArgumentNullException("JWT Key");
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)) throw new ArgumentNullException("JWT Issuer");
        if (string.IsNullOrWhiteSpace(jwtOptions.Audience)) throw new ArgumentNullException("JWT Audience");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var error = new ErrorResponse(nameof(ErrorCodes.AUTHENTICATION_FAILED), ErrorCodes.AUTHENTICATION_FAILED);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(error, AppJsonOptions.Default));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var error = new ErrorResponse(nameof(ErrorCodes.AUTHORIZATION_FAILED), ErrorCodes.AUTHORIZATION_FAILED);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(error, AppJsonOptions.Default));
                    }
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminPolicy, policy =>
                policy.RequireRole(Roles.Administrator))
            .AddPolicy(Policies.PatientPolicy, policy =>
                policy.RequireRole(Roles.Patient));
        return services;
    }

    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>()?
                            .Where(origin => !string.IsNullOrWhiteSpace(origin))
                            .Select(origin => origin.TrimEnd('/'))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            allowedOrigins =
            [
                "http://localhost",
                "https://localhost"
            ];
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireDigit = true
            };

        }).AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<AuthenticationDbContext>()
          .AddSignInManager()
          .AddDefaultTokenProviders();
        return services;
    }
}