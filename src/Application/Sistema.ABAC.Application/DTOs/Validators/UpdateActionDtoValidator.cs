using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class UpdateActionDtoValidator : AbstractValidator<UpdateActionDto>
{
    public UpdateActionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la acción es requerido.")
            .MaximumLength(100).WithMessage("El nombre de la acción no puede exceder 100 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars()
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
