using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/appointments")]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Dsw2026Tpi.CrossCutting.Identity.Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByDate([FromQuery] string date)
    {
       if (!DateOnly.TryParseExact(
    date,
    "yyyy-MM-dd",
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out var parsedDate))
{
    throw new ValidationException()
        .WithDetail("date", "formato inválido, se requiere YYYY-MM-DD");
}

        var appointments = await _service.GetByDate(parsedDate);
        return Ok(appointments);
    }

    [HttpPost]
[Authorize(Policy = Dsw2026Tpi.CrossCutting.Identity.Policies.PatientPolicy)]
[ProducesResponseType(typeof(AppointmentModel.CreateResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Create([FromBody] AppointmentModel.CreateRequest request)
{
    var appointment = await _service.Create(request);
    return Created($"/api/appointments/{appointment.Id}", appointment);
}
}
