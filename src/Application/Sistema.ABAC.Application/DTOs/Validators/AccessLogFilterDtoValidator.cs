using FluentValidation;

namespace Sistema.ABAC.Application.DTOs.Validators;

public class AccessLogFilterDtoValidator : AbstractValidator<AccessLogFilterDto>
{
    private static readonly string[] AllowedSortBy = ["CreatedAt", "Result", "UserId", "ResourceId", "ActionId"];

    public AccessLogFilterDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("El tamaño de página debe estar entre 1 y 200.");

        RuleFor(x => x.SortBy)
            .Must(value => AllowedSortBy.Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El campo de ordenamiento no es válido. Valores permitidos: CreatedAt, Result, UserId, ResourceId, ActionId.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .WithMessage("La fecha final debe ser mayor o igual a la fecha inicial.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}
