using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    public AuthenticationService(UserManager<ApplicationUser> userManager, ISignInService signInManager, RoleManager<IdentityRole> roleManager, JwtService jwtService, ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _logger = logger;

    }
    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new AuthenticationException();
        var passwordIsValid = await _signInManager.CheckPassword(user, request.Password);

        if (!passwordIsValid)
        {
            _logger.LogError("Intento de login administrativo fallido para: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var isAdministrator = await _userManager.IsInRoleAsync(user, Roles.Administrator);

        if (!isAdministrator)
        {
            _logger.LogWarning("Intento de login administrativo de un usuario sin rol Administrador: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var token = _jwtService.GenerateToken(user.UserName!, Roles.Administrator);
      return new LoginAdminModel.Response(token, Roles.Administrator.ToUpperInvariant());
    }
    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Si no existe, se registra automáticamente como paciente.
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Dni = request.Dni,
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                throw new AuthenticationException();
            }

            await _userManager.AddToRoleAsync(user, Roles.Patient);
        }
        else
        {
            if (user.Dni != request.Dni)
                throw new AuthenticationException();
        }

        var isPatient = await _userManager.IsInRoleAsync(user, Roles.Patient);

        if (!isPatient)
        {
            _logger.LogWarning("Intento de login de paciente con un usuario que no posee el rol Paciente: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var token = _jwtService.GenerateToken(user.UserName!, Roles.Patient);

        return new LoginPatientModel.Response(token, Roles.Patient.ToUpperInvariant());
    }
    public async Task<RegisterAdminModel.Response> RegisterAdmin(RegisterAdminModel.Request request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded) throw new ConflictException(nameof(ErrorCodes.REGISTER_USER_CONFLICT),
            ErrorCodes.REGISTER_USER_CONFLICT)
                .WithDetail(result.Errors.Select(e => (e.Code, e.Description)));

        _ = await _userManager.AddToRoleAsync(user, Roles.Administrator);

        _logger.LogInformation("Usuario registrado: {Email}", request.Email);

        return new RegisterAdminModel.Response(request.Email);
    }
}
