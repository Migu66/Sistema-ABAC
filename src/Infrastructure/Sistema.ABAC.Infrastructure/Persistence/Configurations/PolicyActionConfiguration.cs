using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad PolicyAction.
/// Define el esquema de la tabla intermedia entre Policy y Action (many-to-many).
/// </summary>
public class PolicyActionConfiguration : IEntityTypeConfiguration<PolicyAction>
{
    public void Configure(EntityTypeBuilder<PolicyAction> builder)
    {
        // Nombre de la tabla
        builder.ToTable("PolicyActions");

        // Clave primaria
        builder.HasKey(pa => pa.Id);

        // Configuración de propiedades
        builder.Property(pa => pa.PolicyId)
            .IsRequired()
            .HasComment("Identificador de la política");

        builder.Property(pa => pa.ActionId)
            .IsRequired()
            .HasComment("Identificador de la acción");

        // Propiedades heredadas de BaseEntity
        builder.Property(pa => pa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(pa => pa.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(pa => pa.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(pa => pa.PolicyId)
            .HasDatabaseName("IX_PolicyActions_PolicyId");

        builder.HasIndex(pa => pa.ActionId)
            .HasDatabaseName("IX_PolicyActions_ActionId");

        // Índice único compuesto para evitar duplicados (una política no puede tener la misma acción dos veces)
        builder.HasIndex(pa => new { pa.PolicyId, pa.ActionId })
            .IsUnique()
            .HasDatabaseName("IX_PolicyActions_PolicyId_ActionId")
            .HasFilter("[IsDeleted] = 0"); // Solo aplica a registros no eliminados

        builder.HasIndex(pa => pa.IsDeleted)
            .HasDatabaseName("IX_PolicyActions_IsDeleted");

        // Relaciones
        builder.HasOne(pa => pa.Policy)
            .WithMany(p => p.PolicyActions)
            .HasForeignKey(pa => pa.PolicyId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina la política, eliminar sus asociaciones

        builder.HasOne(pa => pa.Action)
            .WithMany(a => a.PolicyActions)
            .HasForeignKey(pa => pa.ActionId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar acción si está en uso
    }
}
