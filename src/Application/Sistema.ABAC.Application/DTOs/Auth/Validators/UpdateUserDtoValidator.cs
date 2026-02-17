using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Auth.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(256).WithMessage("El correo electrónico no puede exceder 256 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El número de teléfono no puede exceder 20 caracteres.")
            .Matches(@"^\+?[0-9\-\s()]+$").WithMessage("El número de teléfono no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("El departamento no puede exceder 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Department));
    }
}
