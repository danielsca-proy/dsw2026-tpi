using Dsw2026Tpi.Api.Services;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Application.Validators;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Data.Providers;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Api.Configurations;

public static class DependencyInjectionConfigurationExtensions
{
    //Inyeccion de las dependencias para el codigo, no olvidar de ponerlos
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        services.AddScoped<IPersistence, PersistenceEf>();
        services.AddSingleton<IHolidayProvider, HolidayProvider>();

        //Servicioss
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<ISpecialityService, SpecialityService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddSingleton<JwtService>();

        //Fluent validation para ahorrar code
        services.AddScoped<IValidator<SpecialityModel.Request>, SpecialityRequestValidator>();
        services.AddScoped<IValidator<AvailabilityModel.Request>, AvailabilityRequestValidator>();
        services.AddScoped<IValidator<AppointmentModel.CreateRequest>, AppointmentCreateRequestValidator>();
        services.AddScoped<IValidator<AppointmentModel.GetByDateQuery>, AppointmentGetByDateQueryValidator>();
        services.AddScoped<IValidator<AppointmentModel.SearchQuery>, AppointmentSearchQueryValidator>();
        services.AddScoped<IValidator<SpecialityModel.GetAllQuery>, SpecialityGetAllValidator>();
        services.AddScoped<IValidator<DoctorModel.Request>, DoctorRequestValidator>();
        services.AddScoped<IValidator<DoctorModel.GetAllQuery>, DoctorGetAllQueryValidator>();

        //Fluent validation pero para register y login
        services.AddScoped<IValidator<LoginAdminModel.Request>, LoginAdminValidator>();
        services.AddScoped<IValidator<LoginPatientModel.Request>, LoginPatientValidator>();

        return services;
    }
}
