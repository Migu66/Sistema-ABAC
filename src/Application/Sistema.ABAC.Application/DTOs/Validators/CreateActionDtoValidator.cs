using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class CreateActionDtoValidator : AbstractValidator<CreateActionDto>
{
    public CreateActionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la acción es requerido.")
            .MaximumLength(100).WithMessage("El nombre de la acción no puede exceder 100 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de la acción es requerido.")
            .MaximumLength(50).WithMessage("El código de la acción no puede exceder 50 caracteres.")
            .MustNotContainControlChars()
            .Matches("^[a-z_][a-z0-9_]*$").WithMessage("El código debe ser en minúsculas, usar snake_case y comenzar con letra.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars()
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
