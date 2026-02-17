using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class CreatePolicyDtoValidator : AbstractValidator<CreatePolicyDto>
{
    public CreatePolicyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la política es requerido.")
            .MaximumLength(200).WithMessage("El nombre de la política no puede exceder 200 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars()
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Effect)
            .IsInEnum().WithMessage("El efecto de la política no es válido.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 999).WithMessage("La prioridad debe estar entre 0 y 999.");

        RuleForEach(x => x.Conditions)
            .SetValidator(new CreatePolicyConditionDtoValidator());
    }
}
