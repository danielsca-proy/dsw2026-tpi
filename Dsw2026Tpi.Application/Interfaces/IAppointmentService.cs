using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentModel.DailyResponse>> GetByDate(DateOnly date);
    Task<AppointmentModel.CreateResponse> Create(AppointmentModel.CreateRequest request);
    Task<Pagination<AppointmentModel.SearchResponse>> Search( Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date, int pageSize, int pageIndex);
    Task Cancel(Guid id, long patientDni);
    Task<IEnumerable<AppointmentModel.PatientResponse>> GetByPatient(long dni);
}