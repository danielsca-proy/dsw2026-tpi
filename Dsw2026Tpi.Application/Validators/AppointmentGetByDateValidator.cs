using System.Globalization;
using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class AppointmentGetByDateQueryValidator : AbstractValidator<AppointmentModel.GetByDateQuery>
{
    public AppointmentGetByDateQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(d => DateOnly.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            .WithMessage("formato inválido, se requiere YYYY-MM-DD");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("debe ser mayor a 0");

        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("no puede ser negativo");

    }
}