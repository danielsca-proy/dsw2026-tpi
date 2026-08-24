using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using FluentValidation;
using ValidationException = Dsw2026Tpi.CrossCutting.Exceptions.ValidationException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/specialties")]
[Authorize]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;
    private readonly IValidator<SpecialityModel.Request> _requestValidator;
    private readonly IValidator<SpecialityModel.GetAllQuery> _getAllValidator;
    public SpecialityController(ISpecialityService service, IValidator<SpecialityModel.Request> requestValidator, IValidator<SpecialityModel.GetAllQuery> getAllValidator)
    {
        _service = service;
        _requestValidator = requestValidator;
        _getAllValidator = getAllValidator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, [FromQuery] string? name = null)
    {
        var query = new SpecialityModel.GetAllQuery(pageSize, pageIndex, name);
        var validation = await _getAllValidator.ValidateAsync(query);
        Invalidez(validation);

        var specialities = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialities);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(typeof(SpecialityModel.Response),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
    {
        var validation = await _requestValidator.ValidateAsync(request);
        Invalidez(validation);

        var speciality = await _service.Create(request);
        return Created($"/api/specialties/{speciality.Id}", speciality);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(typeof(SpecialityModel.Response),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
    {
        var validation = await _requestValidator.ValidateAsync(request);
        Invalidez(validation);

        var speciality = await _service.Update(id, request);
        return Ok(speciality);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }

    //Esto para evitar repetir el bloqeu de codigo en los endpoints
    private static void Invalidez(FluentValidation.Results.ValidationResult validation)
    {
        if (validation.IsValid) return;
        var ex = new ValidationException();
        foreach (var error in validation.Errors)
            ex.WithDetail(error.PropertyName, error.ErrorMessage);
        throw ex;
    }
}
