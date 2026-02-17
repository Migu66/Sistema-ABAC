using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class AssignResourceAttributeDtoValidator : AbstractValidator<AssignResourceAttributeDto>
{
    public AssignResourceAttributeDtoValidator()
    {
        RuleFor(x => x.AttributeId)
            .NotEmpty().WithMessage("El ID del atributo es requerido.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("El valor del atributo es requerido.")
            .MaximumLength(500).WithMessage("El valor del atributo no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();
    }
}
