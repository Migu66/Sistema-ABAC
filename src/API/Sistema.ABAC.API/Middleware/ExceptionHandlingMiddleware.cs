using Microsoft.AspNetCore.Mvc;
using Sistema.ABAC.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Sistema.ABAC.API.Middleware;

/// <summary>
/// Middleware para manejo centralizado de excepciones en toda la aplicación.
/// Captura excepciones no controladas y devuelve respuestas estandarizadas RFC 7807 (ProblemDetails).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Excepción no controlada: {Message}. Request: {Method} {Path}",
                exception.Message,
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            // Excepciones personalizadas del dominio
            NotFoundException notFoundEx => CreateProblemDetails(
                context,
                HttpStatusCode.NotFound,
                "Recurso No Encontrado",
                notFoundEx.Message),
            
            ValidationException validationEx => CreateValidationProblemDetails(
                context,
                validationEx),
            
            ConflictException conflictEx => CreateProblemDetails(
                context,
                HttpStatusCode.Conflict,
                "Conflicto",
                conflictEx.Message),
            
            ForbiddenAccessException forbiddenEx => CreateProblemDetails(
                context,
                HttpStatusCode.Forbidden,
                "Acceso Denegado",
                forbiddenEx.Message),

            // Excepciones genéricas
            ArgumentException or ArgumentNullException => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Petición Inválida",
                _environment.IsDevelopment() ? exception.Message : "Los datos proporcionados no son válidos"),

            InvalidOperationException => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Operación Inválida",
                _environment.IsDevelopment() ? exception.Message : "La operación solicitada no es válida en el estado actual"),

            UnauthorizedAccessException => CreateProblemDetails(
                context,
                HttpStatusCode.Forbidden,
                "Acceso Denegado",
                "No tienes permisos suficientes para realizar esta acción"),

            // Error interno del servidor (no revelar detalles en producción)
            _ => CreateProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                "Error Interno del Servidor",
                _environment.IsDevelopment()
                    ? $"{exception.Message}\n\nStackTrace:\n{exception.StackTrace}"
                    : "Ha ocurrido un error inesperado. Por favor, contacta al administrador del sistema.")
        };

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }

    private ProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException validationException)
    {
        var problemDetails = new ValidationProblemDetails(validationException.Errors)
        {
            Type = "https://httpstatuses.com/400",
            Title = "Error de Validación",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "Se han producido uno o más errores de validación. Revisa los detalles.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }
}

/// <summary>
/// Extensiones para registrar el middleware de manejo de excepciones.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
