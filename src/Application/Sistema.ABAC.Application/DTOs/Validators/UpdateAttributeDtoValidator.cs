using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class UpdateAttributeDtoValidator : AbstractValidator<UpdateAttributeDto>
{
    public UpdateAttributeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del atributo es requerido.")
            .MaximumLength(100).WithMessage("El nombre del atributo no puede exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
