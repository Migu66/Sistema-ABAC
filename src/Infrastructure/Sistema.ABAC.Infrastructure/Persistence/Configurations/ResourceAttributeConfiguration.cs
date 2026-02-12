using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad ResourceAttribute.
/// Define el esquema de la tabla de atributos asignados a recursos.
/// </summary>
public class ResourceAttributeConfiguration : IEntityTypeConfiguration<ResourceAttribute>
{
    public void Configure(EntityTypeBuilder<ResourceAttribute> builder)
    {
        // Nombre de la tabla
        builder.ToTable("ResourceAttributes");

        // Clave primaria
        builder.HasKey(ra => ra.Id);

        // Configuración de propiedades
        builder.Property(ra => ra.ResourceId)
            .IsRequired()
            .HasComment("Identificador del recurso al que se asigna el atributo");

        builder.Property(ra => ra.AttributeId)
            .IsRequired()
            .HasComment("Identificador del atributo que se asigna");

        builder.Property(ra => ra.Value)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Valor concreto del atributo para este recurso");

        // Propiedades heredadas de BaseEntity
        builder.Property(ra => ra.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(ra => ra.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(ra => ra.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(ra => ra.ResourceId)
            .HasDatabaseName("IX_ResourceAttributes_ResourceId");

        builder.HasIndex(ra => ra.AttributeId)
            .HasDatabaseName("IX_ResourceAttributes_AttributeId");

        builder.HasIndex(ra => new { ra.ResourceId, ra.AttributeId })
            .HasDatabaseName("IX_ResourceAttributes_ResourceId_AttributeId");

        builder.HasIndex(ra => ra.IsDeleted)
            .HasDatabaseName("IX_ResourceAttributes_IsDeleted");

        // Relaciones
        builder.HasOne(ra => ra.Resource)
            .WithMany(r => r.ResourceAttributes)
            .HasForeignKey(ra => ra.ResourceId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el recurso, eliminar sus atributos

        builder.HasOne(ra => ra.Attribute)
            .WithMany(a => a.ResourceAttributes)
            .HasForeignKey(ra => ra.AttributeId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar atributo si está en uso
    }
}
