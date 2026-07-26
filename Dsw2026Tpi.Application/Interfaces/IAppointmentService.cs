using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentModel.DailyResponse>> GetByDate(DateOnly date);
    Task<AppointmentModel.CreateResponse> Create(AppointmentModel.CreateRequest request);
// Julia agrega acá (Día 7): Cancel, GetByPatient
}