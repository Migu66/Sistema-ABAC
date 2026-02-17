using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class CreateAttributeDtoValidator : AbstractValidator<CreateAttributeDto>
{
    public CreateAttributeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del atributo es requerido.")
            .MaximumLength(100).WithMessage("El nombre del atributo no puede exceder 100 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("La clave del atributo es requerida.")
            .MaximumLength(100).WithMessage("La clave del atributo no puede exceder 100 caracteres.")
            .MustNotContainControlChars()
            .Matches("^[a-z_][a-z0-9_]*$").WithMessage("La clave debe ser en minúsculas, usar snake_case y comenzar con letra.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de atributo no es válido.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars()
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
