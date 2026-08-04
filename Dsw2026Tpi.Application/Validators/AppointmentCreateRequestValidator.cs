using FluentValidation;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Application.Validators;

public class AppointmentCreateRequestValidator : AbstractValidator<AppointmentModel.CreateRequest>
{
    public AppointmentCreateRequestValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .WithMessage("es obligatorio");

        RuleFor(x => x.AvailabilitySlotId)
            .NotEmpty()
            .WithMessage("es obligatorio");

        RuleFor(x => x.Patient)
            .NotNull()
            .WithMessage("es obligatorio");

        When(x => x.Patient is not null, () =>
        {
            RuleFor(x => x.Patient!.Dni)
                .Must(dni => dni.IsDniValid())
                .WithMessage("debe tener 7 u 8 dígitos");
        });

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("es obligatorio")
            .MinimumLength(5)
            .WithMessage("debe tener al menos 5 caracteres");
    }
}