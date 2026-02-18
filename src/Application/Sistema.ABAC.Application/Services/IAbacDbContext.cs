using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Interfaz del contexto de base de datos ABAC.
/// Define los DbSets necesarios para operaciones de autenticación y gestión de tokens.
/// </summary>
public interface IAbacDbContext
{
    /// <summary>
    /// Tabla de tokens de actualización (refresh tokens).
    /// </summary>
    DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Guarda los cambios realizados en el contexto de manera asíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Número de entidades afectadas.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
