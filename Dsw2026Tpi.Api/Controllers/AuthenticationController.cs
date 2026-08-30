using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using FluentValidation;
using ValidationException = Dsw2026Tpi.CrossCutting.Exceptions.ValidationException;
using Microsoft.AspNetCore.Mvc;
using Dsw2026Tpi.Api.Configurations;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/auth")]
public class AuthenticationController : AppController
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IValidator<LoginAdminModel.Request> _loginAdminValidator;
    private readonly IValidator<LoginPatientModel.Request> _loginPatientValidator;
    private readonly IValidator<RegisterAdminModel.Request> _registerValidator;

    public AuthenticationController(IAuthenticationService authenticationService, IValidator<LoginAdminModel.Request> loginAdminValidator, IValidator<LoginPatientModel.Request> loginPatientValidator, IValidator<RegisterAdminModel.Request> registerValidator) 
    {
        _authenticationService = authenticationService;
        _loginAdminValidator = loginAdminValidator;
        _loginPatientValidator = loginPatientValidator;
        _registerValidator = registerValidator;
    }

    [HttpPost("admin/login")]
    [EnableRateLimiting(RateLimitPolicies.AdminLogin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginAdminModel.Request request)
    {
        var validation = await _loginAdminValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.LoginAdmin(request);
        return Ok(result);
    }

    [HttpPost("patient/login")]
    [EnableRateLimiting(RateLimitPolicies.PatientLogin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LoginPatient([FromBody] LoginPatientModel.Request request)
    {
        var validation = await _loginPatientValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.LoginPatient(request);
        return Ok(result);
    }

    //Este metodo sera eliminado a futuro.
    [HttpPost("admin/register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterAdminModel.Request request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.RegisterAdmin(request);
        return Ok(result.Email);
    }
}
