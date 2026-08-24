using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<AvailabilityModel.Response>> Create(AvailabilityModel.Request request);
    Task<List<AvailabilityModel.Response>> Update(AvailabilityModel.Request request);
}