using FluentValidation;
using System.Text.RegularExpressions;

namespace Sistema.ABAC.Application.DTOs.Validators;

public static partial class InputSanitizationExtensions
{
    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("[\\u0000-\\u001F\\u007F]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ControlCharsRegex();

    public static IRuleBuilderOptions<T, string> MustNotContainHtml<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => string.IsNullOrWhiteSpace(value) || !HtmlTagRegex().IsMatch(value))
            .WithMessage("El valor no debe contener etiquetas HTML.");
    }

    public static IRuleBuilderOptions<T, string> MustNotContainControlChars<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => string.IsNullOrWhiteSpace(value) || !ControlCharsRegex().IsMatch(value))
            .WithMessage("El valor contiene caracteres no permitidos.");
    }
}
