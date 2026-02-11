namespace Sistema.ABAC.Application.DTOs.Common;

/// <summary>
/// DTO genérico para respuestas paginadas.
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// Lista de elementos de la página actual.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Número de página actual (base 1).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamaño de página (elementos por página).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de elementos en todos los resultados.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas disponibles.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indica si hay página anterior.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si hay página siguiente.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
