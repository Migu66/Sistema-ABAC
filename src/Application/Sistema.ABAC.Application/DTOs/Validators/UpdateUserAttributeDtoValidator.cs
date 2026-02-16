using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class UpdateUserAttributeDtoValidator : AbstractValidator<UpdateUserAttributeDto>
{
    public UpdateUserAttributeDtoValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("El valor del atributo es requerido.")
            .MaximumLength(500).WithMessage("El valor del atributo no puede exceder 500 caracteres.")
            .MustNotContainHtml()
            .MustNotContainControlChars();

        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom!.Value)
            .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio.")
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
    }
}
