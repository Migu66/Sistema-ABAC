using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Action.
/// Define el esquema de la tabla de acciones del sistema.
/// </summary>
public class ActionConfiguration : IEntityTypeConfiguration<Domain.Entities.Action>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Action> builder)
    {
        // Nombre de la tabla
        builder.ToTable("Actions");

        // Clave primaria
        builder.HasKey(a => a.Id);

        // Configuración de propiedades
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Nombre descriptivo de la acción");

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Código único de la acción usado en evaluación");

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasComment("Descripción detallada de lo que permite hacer esta acción");

        // Propiedades heredadas de BaseEntity
        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(a => a.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(a => a.Code)
            .IsUnique()
            .HasDatabaseName("IX_Actions_Code")
            .HasFilter("[IsDeleted] = 0"); // Solo aplica a registros no eliminados

        builder.HasIndex(a => a.Name)
            .HasDatabaseName("IX_Actions_Name");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_Actions_IsDeleted");

        // Relaciones
        builder.HasMany(a => a.PolicyActions)
            .WithOne(pa => pa.Action)
            .HasForeignKey(pa => pa.ActionId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar si hay políticas asociadas

        builder.HasMany(a => a.AccessLogs)
            .WithOne(al => al.Action)
            .HasForeignKey(al => al.ActionId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar si hay logs
    }
}
