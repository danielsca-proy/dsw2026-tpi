using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ValidationException = Dsw2026Tpi.CrossCutting.Exceptions.ValidationException;

namespace Dsw2026Tpi.Api.Controllers;
[Route("api/availabilities")]
[Authorize(Policy = Dsw2026Tpi.CrossCutting.Identity.Policies.AdminPolicy)]
public class AvailabilityController : AppController
{
    private readonly IAvailabilityService _service;
    private readonly IValidator<AvailabilityModel.Request> _requestValidator;

    public AvailabilityController(IAvailabilityService service, IValidator<AvailabilityModel.Request> requestValidation)
    {
        _service = service;
        _requestValidator = requestValidation;
    }

    //Metodo para crear una disponibilidad
    [HttpPost]
    [ProducesResponseType(typeof(List<AvailabilityModel.Response>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] AvailabilityModel.Request request)
    {
        var validation = await _requestValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _service.Create(request);
        return Created($"/api/availabilities/{result.First().Id}", result);
    }

    //Metodo para actualizar una disponibilidad
    [HttpPut]
    [ProducesResponseType(typeof(List<AvailabilityModel.Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
    {
        var validation = await _requestValidator.ValidateAsync(request);
        Invalidez(validation);

        var result = await _service.Update(request);
        return Ok(result);
    }

    //Mismo metodo para codigo
    private static void Invalidez(FluentValidation.Results.ValidationResult validation)
    {
        if (validation.IsValid) return;
        var ex = new ValidationException();
        foreach (var error in validation.Errors)
            ex.WithDetail(error.PropertyName, error.ErrorMessage);
        throw ex;
    }
}