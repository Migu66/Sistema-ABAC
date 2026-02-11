namespace Sistema.ABAC.Application.DTOs.Common;

/// <summary>
/// DTO genérico para respuestas de la API.
/// </summary>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Datos de la respuesta.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Lista de errores (si los hay).
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Marca de tiempo de la respuesta.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Crea una respuesta exitosa.
    /// </summary>
    public static ApiResponseDto<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message ?? "Operación exitosa",
            Data = data
        };
    }

    /// <summary>
    /// Crea una respuesta de error.
    /// </summary>
    public static ApiResponseDto<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
