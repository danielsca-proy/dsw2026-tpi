using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class SpecialityGetAllValidator : AbstractValidator<SpecialityModel.GetAllQuery>
{
    public SpecialityGetAllValidator()
    {
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("debe ser mayor a 0");
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("no puede ser negativo");
        RuleFor(x => x.Name)
            .Length(3, 100).WithMessage("debe tener entre 3 y 100 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
    }
}