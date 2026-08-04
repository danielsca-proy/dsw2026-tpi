using FluentValidation;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterAdminModel.Request>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("obligatorio")
            .Must(email => email.IsEmailValid())
            .WithMessage("formato inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("obligatorio")
            .MinimumLength(8).WithMessage("mínimo 8 caracteres");
    }
}
