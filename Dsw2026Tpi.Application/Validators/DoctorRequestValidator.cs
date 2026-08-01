using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class DoctorRequestValidator : AbstractValidator<DoctorModel.Request>
{
    public DoctorRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 100);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SpecialityId).NotEqual(Guid.Empty).WithMessage("es obligatorio");
    }
}
