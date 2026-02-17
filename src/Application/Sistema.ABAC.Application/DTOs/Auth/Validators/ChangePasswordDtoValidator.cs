using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es requerida.");

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
