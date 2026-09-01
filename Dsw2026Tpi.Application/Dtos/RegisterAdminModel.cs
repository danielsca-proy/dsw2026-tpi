using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record RegisterAdminModel
{
    public record Request(string Email, string Password);
    public record Response(string Email);
}
