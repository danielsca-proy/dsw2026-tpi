using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dsw2026Tpi.CrossCutting.Exceptions;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/specialties")]
[Authorize(Policy = Policies.AdminPolicy)]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;

    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
    {
        if (pageSize <= 0)
            throw new ValidationException().WithDetail("pageSize", "debe ser mayor a 0");

        if (pageIndex < 0)
            throw new ValidationException().WithDetail("pageIndex", "no puede ser negativo");

        if (!string.IsNullOrWhiteSpace(name) && (name.Length < 3 || name.Length > 100))
            throw new ValidationException().WithDetail("name", "debe tener entre 3 y 100 caracteres");

        var specialities = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialities);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpecialityModel.Response),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
    [FromBody] SpecialityModel.Request request)
    {
        var speciality = await _service.Create(request);

        return Created($"/api/specialties/{speciality.Id}", speciality);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SpecialityModel.Response),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] SpecialityModel.Request request)
    {
        var speciality = await _service.Update(id, request);

        return Ok(speciality);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);

        return NoContent();
    }

}
