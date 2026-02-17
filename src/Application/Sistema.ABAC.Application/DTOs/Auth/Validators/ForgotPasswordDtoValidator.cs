using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder 256 caracteres.");
    }
}
