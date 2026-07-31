using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class SpecialityRequestValidator : AbstractValidator<SpecialityModel.Request>
{
    public SpecialityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(3, 100);
        RuleFor(x => x.Description).NotEmpty().Length(10, 100);
    }
}