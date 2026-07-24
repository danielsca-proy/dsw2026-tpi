using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
[Authorize(Policy = Policies.AdminPolicy)]
public class DoctorController : AppController
{
    private readonly IDoctorService _service;

    public DoctorController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageSize,
        [FromQuery] int pageIndex,
        [FromQuery] string? name = null)
    {
        var doctors = await _service.GetAll(
            pageSize, 
            pageIndex, 
            name);

        return Ok(doctors);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(DoctorModel.Response), 
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] DoctorModel.Request request)
    {
        var doctor = await _service.Create(request);

        return Created($"/api/doctors/{doctor.Id}", doctor);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(DoctorModel.Response),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] DoctorModel.Request request)
    {
        var doctor = await _service.Update(id, request);

        return Ok(doctor);
    }
}