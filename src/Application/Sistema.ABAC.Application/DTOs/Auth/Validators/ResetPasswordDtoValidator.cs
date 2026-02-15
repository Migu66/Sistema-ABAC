using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder 256 caracteres.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token de recuperación es requerido.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La nueva contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La nueva contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La nueva contraseña debe contener al menos un número.")
            .Matches("[@$!%*?&#^()_]").WithMessage("La nueva contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.NewPassword).WithMessage("La confirmación de contraseña no coincide con la nueva contraseña.");
    }
}
