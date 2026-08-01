using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;


namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Inicializar con un logger simple antes de construir el host
        Log.Logger = new LoggerConfiguration()
           .WriteTo.Console()
           .CreateBootstrapLogger();

        try
        {
            Log.Information("Iniciando aplicación Dsw2026Tpi.Api");

            var builder = WebApplication.CreateBuilder(args);

            //Configuraciones personalizadas
            builder.AddSerilogConfiguration();
            builder.Services.AddAppIdentity();
            builder.Services.AddAppAuthentication(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddApplicationPersistence(builder.Configuration);
            builder.Services.AddAppCors(builder.Configuration);
            builder.Services.AddAppDependencies();
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            //Crea los roles en la base de datos si no existen
            await app.SeedRolesAsync();
          
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                const string email = "admin@utn.com";
                const string password = "Admin123!";
                const long dni = 99999999;

                var admin = await userManager.FindByEmailAsync(email);

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        Dni = dni,
                        Deleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(admin, password);

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    await userManager.AddToRoleAsync(admin, Roles.Administrator);
                }
            }

            app.UseSerilogRequestLogging();

            if (app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //Aqui deberia agregarse la cuenta admin una sola vez o en el metodo RoleSeeding?
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.MapControllers();
            app.MapHealthChecks("/health-check");

            Log.Information("Aplicación iniciada correctamente");

            await app.RunAsync();
        }
        catch (HostAbortedException)
        {
            Log.Information("El host fue abortado (normal durante migraciones de EF Core)");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación falló al iniciar");
            throw;
        }
        finally
        {
            Log.Information("Cerrando aplicación");
            await Log.CloseAndFlushAsync();
        }
    }
}

