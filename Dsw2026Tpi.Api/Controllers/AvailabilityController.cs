using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/availabilities")]
[Authorize(Policy = Dsw2026Tpi.CrossCutting.Identity.Policies.AdminPolicy)]
public class AvailabilityController : AppController
{
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AvailabilityModel.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] AvailabilityModel.Request request)
    {
        if (request.StartTime >= request.EndTime)
        {
            return BadRequest("La hora de inicio no puede ser antes de la de fin");
        }

        var result = await _service.Create(request);

        return Created($"/api/availabilities/{result.Id}", result);
    }
}
