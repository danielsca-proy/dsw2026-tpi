using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record RegisterAdminModel //A futuro sera eliminado
{
    public record Request(string Email, string Password);
    public record Response(string Email);
}
