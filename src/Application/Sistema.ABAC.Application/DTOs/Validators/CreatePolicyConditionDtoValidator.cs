using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class CreatePolicyConditionDtoValidator : AbstractValidator<CreatePolicyConditionDto>
{
    public CreatePolicyConditionDtoValidator()
    {
        RuleFor(x => x.AttributeType)
            .NotEmpty().WithMessage("El tipo de atributo es requerido.")
            .MaximumLength(50).WithMessage("El tipo de atributo no puede exceder 50 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.AttributeKey)
            .NotEmpty().WithMessage("La clave del atributo es requerida.")
            .MaximumLength(100).WithMessage("La clave del atributo no puede exceder 100 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("El operador no es válido.");

        RuleFor(x => x.ExpectedValue)
            .NotEmpty().WithMessage("El valor esperado es requerido.")
            .MaximumLength(500).WithMessage("El valor esperado no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();
    }
}
