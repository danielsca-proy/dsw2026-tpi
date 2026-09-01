using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Helpers;
using FluentValidation;

namespace Dsw2026Tpi.Application.Validators;
public class LoginPatientValidator : AbstractValidator<LoginPatientModel.Request>
{
    public LoginPatientValidator()
    {
        RuleFor(x => x.Email)
            .Must(email => email.IsEmailValid())
            .WithMessage("formato inválido");

        RuleFor(x => x.Dni)
            .Must(dni => dni.IsDniValid())
            .WithMessage("debe tener 7 u 8 dígitos");
    }
}