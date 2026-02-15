using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class UpdateResourceDtoValidator : AbstractValidator<UpdateResourceDto>
{
    public UpdateResourceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del recurso es requerido.")
            .MaximumLength(200).WithMessage("El nombre del recurso no puede exceder 200 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo del recurso es requerido.")
            .MaximumLength(100).WithMessage("El tipo del recurso no puede exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
