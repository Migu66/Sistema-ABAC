using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Domain.Interfaces;

/// <summary>
/// Interfaz Unit of Work para coordinar operaciones entre múltiples repositorios
/// y gestionar transacciones de base de datos de manera consistente.
/// </summary>
/// <remarks>
/// El patrón Unit of Work garantiza que:
/// - Todos los cambios se guardan juntos o se revierten juntos (atomicidad)
/// - Se mantiene una única instancia de DbContext durante una operación
/// - Las transacciones están bien coordinadas entre repositorios
/// 
/// Uso típico:
/// <code>
/// await using var transaction = await _unitOfWork.BeginTransactionAsync();
/// try
/// {
///     await _unitOfWork.Users.AddAsync(user);
///     await _unitOfWork.UserAttributes.AddRangeAsync(attributes);
///     await _unitOfWork.SaveChangesAsync();
///     await transaction.CommitAsync();
/// }
/// catch
/// {
///     await transaction.RollbackAsync();
///     throw;
/// }
/// </code>
/// </remarks>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Repositorio para usuarios del sistema.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Repositorio para definiciones de atributos.
    /// </summary>
    IAttributeRepository Attributes { get; }

    /// <summary>
    /// Repositorio para recursos protegidos.
    /// </summary>
    IResourceRepository Resources { get; }

    /// <summary>
    /// Repositorio para acciones del sistema.
    /// </summary>
    IActionRepository Actions { get; }

    /// <summary>
    /// Repositorio para políticas ABAC.
    /// </summary>
    IPolicyRepository Policies { get; }

    /// <summary>
    /// Repositorio para logs de auditoría.
    /// </summary>
    IAccessLogRepository AccessLogs { get; }

    /// <summary>
    /// Repositorio para atributos de usuarios (asignaciones).
    /// </summary>
    IRepository<UserAttribute> UserAttributes { get; }

    /// <summary>
    /// Repositorio para atributos de recursos (asignaciones).
    /// </summary>
    IRepository<ResourceAttribute> ResourceAttributes { get; }

    /// <summary>
    /// Repositorio para condiciones de políticas.
    /// </summary>
    IRepository<PolicyCondition> PolicyConditions { get; }

    /// <summary>
    /// Repositorio para asociaciones entre políticas y acciones.
    /// </summary>
    IRepository<PolicyAction> PolicyActions { get; }

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una nueva transacción de base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Objeto de transacción que debe ser committed o rolled back</returns>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz para manejar transacciones de base de datos.
/// </summary>
public interface IDbTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Confirma la transacción, guardando todos los cambios en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción, descartando todos los cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
