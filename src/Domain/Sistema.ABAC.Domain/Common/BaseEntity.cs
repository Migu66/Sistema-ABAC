namespace Sistema.ABAC.Domain.Common;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Proporciona propiedades comunes de auditoría y soporte para Soft Delete.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la entidad (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización de la entidad (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indica si la entidad ha sido eliminada lógicamente (Soft Delete).
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Constructor protegido que inicializa los valores por defecto.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}
