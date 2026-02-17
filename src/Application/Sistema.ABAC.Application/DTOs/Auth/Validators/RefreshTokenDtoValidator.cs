using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

/// <summary>
/// Validador para RefreshTokenDto usando FluentValidation.
/// </summary>
public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("El token de acceso es requerido.")
            .MinimumLength(20).WithMessage("El token de acceso no es válido.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El token de actualización es requerido.")
            .MinimumLength(20).WithMessage("El token de actualización no es válido.");
    }
}
