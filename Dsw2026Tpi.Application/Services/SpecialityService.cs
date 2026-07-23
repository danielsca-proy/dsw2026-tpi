using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class SpecialityService : ISpecialityService
    {
        private readonly IPersistence _persistence;

        public SpecialityService(IPersistence persistence)
        {
            _persistence = persistence;
        }
        public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
                throw new ValidationException().WithDetail("name", "debe tener entre 3 y 100 caracteres");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
                throw new ValidationException().WithDetail("description", "debe tener entre 10 y 100 caracteres");

            var speciality = new Speciality(request.Name, request.Description);
            await _persistence.Add<Speciality>(speciality);
            return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
        }

        public async Task Delete(Guid id)
        {
            var speciality = await _persistence.GetById<Speciality>(id);

            if (speciality is null)
                throw new EntityNotFoundException("Speciality not found");

            speciality.MarkAsDeleted();
            await _persistence.Update<Speciality>(speciality);
        }

        public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
        {
            var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex,
                s => !s.Deleted && (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)), x => x.Name);

            return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
        }

        public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
        {
            var speciality = await _persistence.GetById<Speciality>(id);


            if (speciality is null)
                throw new EntityNotFoundException("Speciality not found");

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
                throw new ValidationException().WithDetail("name", "debe tener entre 3 y 100 caracteres");

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
                throw new ValidationException().WithDetail("description", "debe tener entre 10 y 100 caracteres");


            if (speciality is null)
                throw new EntityNotFoundException("Speciality not found");

            speciality.UpdateDetails(request.Name, request.Description);
            await _persistence.Update<Speciality>(speciality);

            return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
        }
    }
}
