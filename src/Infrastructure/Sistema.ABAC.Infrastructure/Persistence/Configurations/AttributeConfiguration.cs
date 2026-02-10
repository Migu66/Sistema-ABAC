using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Attribute.
/// Define el esquema de la tabla, restricciones, índices y relaciones.
/// </summary>
public class AttributeConfiguration : IEntityTypeConfiguration<Domain.Entities.Attribute>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Attribute> builder)
    {
        // Nombre de la tabla
        builder.ToTable("Attributes");

        // Clave primaria (ya está configurada en BaseEntity, pero se puede especificar explícitamente)
        builder.HasKey(a => a.Id);

        // Configuración de propiedades
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Nombre descriptivo del atributo");

        builder.Property(a => a.Key)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Clave única del atributo usada en evaluación de políticas");

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>() // Almacena el enum como string en la BD
            .HasMaxLength(50)
            .HasComment("Tipo de dato del atributo (String, Number, Boolean, DateTime)");

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasComment("Descripción detallada del propósito del atributo");

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
        builder.HasIndex(a => a.Key)
            .IsUnique()
            .HasDatabaseName("IX_Attributes_Key")
            .HasFilter("[IsDeleted] = 0"); // Solo aplica a registros no eliminados

        builder.HasIndex(a => a.Name)
            .HasDatabaseName("IX_Attributes_Name");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_Attributes_IsDeleted");

        // Relaciones
        builder.HasMany(a => a.UserAttributes)
            .WithOne(ua => ua.Attribute)
            .HasForeignKey(ua => ua.AttributeId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar si hay valores asignados

        builder.HasMany(a => a.ResourceAttributes)
            .WithOne(ra => ra.Attribute)
            .HasForeignKey(ra => ra.AttributeId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar si hay valores asignados
    }
}
