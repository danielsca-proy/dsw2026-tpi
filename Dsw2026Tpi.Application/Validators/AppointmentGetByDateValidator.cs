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
    }
}