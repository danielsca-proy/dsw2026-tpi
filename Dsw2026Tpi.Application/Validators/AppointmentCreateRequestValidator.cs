using FluentValidation;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Validators;

public class AppointmentCreateRequestValidator : AbstractValidator<AppointmentModel.CreateRequest>
{
    public AppointmentCreateRequestValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.AvailabilitySlotId).NotEmpty();
        RuleFor(x => x.PatientDni).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(5);
    }
}