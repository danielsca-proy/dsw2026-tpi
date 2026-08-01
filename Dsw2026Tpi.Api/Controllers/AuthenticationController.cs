using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using FluentValidation;
using ValidationException = Dsw2026Tpi.CrossCutting.Exceptions.ValidationException;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/auth")]
public class AuthenticationController : AppController
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IValidator<LoginAdminModel.Request> _loginAdminValidator;
    private readonly IValidator<LoginPatientModel.Request> _loginPatientValidator;
    private readonly IValidator<RegisterModel.Request> _registerValidator;

    public AuthenticationController(IAuthenticationService authenticationService, IValidator<LoginAdminModel.Request> loginAdminValidator, IValidator<LoginPatientModel.Request> loginPatientValidator, IValidator<RegisterModel.Request> registerValidator) 
    {
        _authenticationService = authenticationService;
        _loginAdminValidator = loginAdminValidator;
        _loginPatientValidator = loginPatientValidator;
        _registerValidator = registerValidator;
    }

    //Metodo para registrar un adminstrador
    [HttpPost("admin/register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterModel.Request request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.Register(request);
        return Ok(result.Email); 
    }

    //Metodo para loguear un administrador
    [HttpPost("admin/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginAdminModel.Request request)
    {
        var validation = await _loginAdminValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.LoginAdmin(request);
        return Ok(result);
    }

    //Metodo para loguear una paciente, aqui si no existe se crea uno
    [HttpPost("patient/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginPatient([FromBody] LoginPatientModel.Request request)
    {
        var validation = await _loginPatientValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _authenticationService.LoginPatient(request);
        return Ok(result);
    }

    //Mismo codigo para ahorrar codigo
    private static void Invalidez(FluentValidation.Results.ValidationResult validation)
    {
        if (validation.IsValid) return;
        var ex = new ValidationException();
        foreach (var error in validation.Errors)
            ex.WithDetail(error.PropertyName, error.ErrorMessage);
        throw ex;
    }
}
