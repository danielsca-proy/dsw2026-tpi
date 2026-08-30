using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class AvailabilityRequestValidator : AbstractValidator<AvailabilityModel.Request>
{
    public AvailabilityRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("formato inválido, se requiere Guid");

        RuleFor(x => x.Days)
            .NotEmpty()
            .WithMessage("se requiere al menos un dia con horario");

        RuleForEach(x => x.Days).SetValidator(new DayRequestValidator());
    }
}

public class DayRequestValidator : AbstractValidator<AvailabilityModel.DayRequest>
{
    public DayRequestValidator()
    {
        RuleFor(x => x.Day).NotEmpty();

        RuleFor(x => x.StartTime)
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("formato inválido, se requiere HH:mm");

        RuleFor(x => x.EndTime)
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("formato inválido, se requiere HH:mm");
    }
}