using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sistema.ABAC.Domain.Common;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de EF Core que maneja automáticamente la auditoría de entidades.
/// Se ejecuta antes de guardar cambios en la base de datos para:
/// - Establecer CreatedAt en entidades nuevas
/// - Actualizar UpdatedAt en entidades modificadas
/// - Convertir eliminaciones físicas en eliminaciones lógicas (Soft Delete)
/// </summary>
/// <remarks>
/// Este interceptor implementa el patrón de auditoría automática, garantizando que
/// todas las entidades que heredan de BaseEntity o tienen propiedades de auditoría
/// mantengan metadatos precisos sobre su ciclo de vida sin intervención manual del desarrollador.
/// </remarks>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Se ejecuta antes de guardar cambios de forma síncrona.
    /// Actualiza las propiedades de auditoría de las entidades modificadas.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Se ejecuta antes de guardar cambios de forma asíncrona.
    /// Actualiza las propiedades de auditoría de las entidades modificadas.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Actualiza las propiedades de auditoría de todas las entidades que están siendo rastreadas.
    /// </summary>
    /// <param name="context">El DbContext actual con las entidades rastreadas.</param>
    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var utcNow = DateTime.UtcNow;

        // Obtener todas las entradas de entidades que están siendo rastreadas
        var entries = context.ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // ============================================================
            // MANEJO DE ENTIDADES QUE HEREDAN DE BASEENTITY
            // ============================================================
            if (entry.Entity is BaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Entidad nueva: establecer CreatedAt
                        baseEntity.CreatedAt = utcNow;
                        baseEntity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Entidad modificada: actualizar UpdatedAt
                        baseEntity.UpdatedAt = utcNow;
                        break;

                    case EntityState.Deleted:
                        // Convertir eliminación física en eliminación lógica (Soft Delete)
                        entry.State = EntityState.Modified;
                        baseEntity.IsDeleted = true;
                        baseEntity.UpdatedAt = utcNow;
                        break;
                }
            }

            // ============================================================
            // MANEJO ESPECIAL PARA USER (hereda de IdentityUser, no BaseEntity)
            // ============================================================
            else if (entry.Entity is User user)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        user.CreatedAt = utcNow;
                        user.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        user.UpdatedAt = utcNow;
                        break;

                    case EntityState.Deleted:
                        // Convertir eliminación física en eliminación lógica (Soft Delete)
                        entry.State = EntityState.Modified;
                        user.IsDeleted = true;
                        user.UpdatedAt = utcNow;
                        break;
                }
            }
        }
    }
}
