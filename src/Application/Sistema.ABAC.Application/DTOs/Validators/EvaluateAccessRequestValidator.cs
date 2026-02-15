using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class EvaluateAccessRequestValidator : AbstractValidator<EvaluateAccessRequest>
{
    public EvaluateAccessRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido.");

        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("El ID del recurso es requerido.");

        RuleFor(x => x.ActionId)
            .NotEmpty().WithMessage("El ID de la acción es requerido.");
    }
}
