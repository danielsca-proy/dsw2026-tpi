using Dsw2026Tpi.Api.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

/// <summary>
/// Clase base para configuraciones generales de controladores
/// </summary>
[ApiController]
[EnableRateLimiting(RateLimitPolicies.General)] //Para evitar tener que hacerlo en todos los controladores, lo hacemos aqui
[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
public abstract class AppController : ControllerBase
{
}

