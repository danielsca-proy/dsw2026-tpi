using FluentValidation;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Helpers;

namespace Dsw2026Tpi.Application.Validators;

public class LoginAdminValidator : AbstractValidator<LoginAdminModel.Request>
{
    public LoginAdminValidator()
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