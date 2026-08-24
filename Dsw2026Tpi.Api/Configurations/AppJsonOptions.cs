using System.Text.Json;

namespace Dsw2026Tpi.Api.Configurations;

public static class AppJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}