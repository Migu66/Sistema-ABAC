using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

/// <summary>
/// Validador para RegisterDto usando FluentValidation.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede tener más de 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(100).WithMessage("El correo electrónico no puede tener más de 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[@$!%*?&#^()]").WithMessage("La contraseña debe contener al menos un carácter especial (@$!%*?&#^()).");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido.")
            .MinimumLength(3).WithMessage("El nombre completo debe tener al menos 3 caracteres.")
            .MaximumLength(100).WithMessage("El nombre completo no puede tener más de 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El número de teléfono no es válido.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
