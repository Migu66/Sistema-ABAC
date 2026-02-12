using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Resource.
/// Define el esquema de la tabla de recursos protegidos.
/// </summary>
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        // Nombre de la tabla
        builder.ToTable("Resources");

        // Clave primaria
        builder.HasKey(r => r.Id);

        // Configuración de propiedades
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Nombre descriptivo del recurso");

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Tipo o categoría del recurso (documento, endpoint, vista, etc.)");

        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .HasComment("Descripción detallada del recurso y su contenido");

        // Propiedades heredadas de BaseEntity
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(r => r.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(r => r.Name)
            .HasDatabaseName("IX_Resources_Name");

        builder.HasIndex(r => r.Type)
            .HasDatabaseName("IX_Resources_Type");

        builder.HasIndex(r => r.IsDeleted)
            .HasDatabaseName("IX_Resources_IsDeleted");

        builder.HasIndex(r => new { r.Type, r.IsDeleted })
            .HasDatabaseName("IX_Resources_Type_IsDeleted");

        // Relaciones
        builder.HasMany(r => r.ResourceAttributes)
            .WithOne(ra => ra.Resource)
            .HasForeignKey(ra => ra.ResourceId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el recurso, eliminar sus atributos

        builder.HasMany(r => r.AccessLogs)
            .WithOne(al => al.Resource)
            .HasForeignKey(al => al.ResourceId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar si hay logs
    }
}
