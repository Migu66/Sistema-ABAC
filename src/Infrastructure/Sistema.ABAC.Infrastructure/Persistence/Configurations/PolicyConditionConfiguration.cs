using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad PolicyCondition.
/// Define el esquema de la tabla de condiciones de políticas.
/// </summary>
public class PolicyConditionConfiguration : IEntityTypeConfiguration<PolicyCondition>
{
    public void Configure(EntityTypeBuilder<PolicyCondition> builder)
    {
        // Nombre de la tabla
        builder.ToTable("PolicyConditions");

        // Clave primaria
        builder.HasKey(pc => pc.Id);

        // Configuración de propiedades
        builder.Property(pc => pc.PolicyId)
            .IsRequired()
            .HasComment("Identificador de la política a la que pertenece esta condición");

        builder.Property(pc => pc.AttributeType)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Tipo de atributo: Subject, Resource o Environment");

        builder.Property(pc => pc.AttributeKey)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Clave del atributo que se evaluará");

        builder.Property(pc => pc.Operator)
            .IsRequired()
            .HasConversion<string>() // Almacena el enum como string en la BD
            .HasMaxLength(50)
            .HasComment("Operador de comparación (Equals, NotEquals, GreaterThan, etc.)");

        builder.Property(pc => pc.ExpectedValue)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Valor esperado contra el cual se comparará el atributo");

        // Propiedades heredadas de BaseEntity
        builder.Property(pc => pc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(pc => pc.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(pc => pc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(pc => pc.PolicyId)
            .HasDatabaseName("IX_PolicyConditions_PolicyId");

        builder.HasIndex(pc => pc.AttributeKey)
            .HasDatabaseName("IX_PolicyConditions_AttributeKey");

        builder.HasIndex(pc => new { pc.PolicyId, pc.AttributeType })
            .HasDatabaseName("IX_PolicyConditions_PolicyId_AttributeType");

        builder.HasIndex(pc => pc.IsDeleted)
            .HasDatabaseName("IX_PolicyConditions_IsDeleted");

        // Relaciones
        builder.HasOne(pc => pc.Policy)
            .WithMany(p => p.Conditions)
            .HasForeignKey(pc => pc.PolicyId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina la política, eliminar sus condiciones
    }
}
