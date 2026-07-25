using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentModel.DailyResponse>> GetByDate(DateOnly date);
    // Julia agrega acá: Create, Cancel, GetByPatient
}