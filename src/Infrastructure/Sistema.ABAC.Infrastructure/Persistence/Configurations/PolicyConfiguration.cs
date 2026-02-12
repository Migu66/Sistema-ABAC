using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad Policy.
/// Define el esquema de la tabla de políticas ABAC.
/// </summary>
public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        // Nombre de la tabla
        builder.ToTable("Policies");

        // Clave primaria
        builder.HasKey(p => p.Id);

        // Configuración de propiedades
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Nombre descriptivo de la política");

        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .HasComment("Descripción detallada del propósito de la política");

        builder.Property(p => p.Effect)
            .IsRequired()
            .HasConversion<string>() // Almacena el enum como string en la BD
            .HasMaxLength(20)
            .HasComment("Efecto de la política cuando se cumplen las condiciones (Permit o Deny)");

        builder.Property(p => p.Priority)
            .IsRequired()
            .HasDefaultValue(100)
            .HasComment("Prioridad numérica para resolver conflictos (mayor = más prioritaria)");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indica si la política está activa y debe ser evaluada");

        // Propiedades heredadas de BaseEntity
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(p => p.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Policies_Name");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Policies_IsActive");

        builder.HasIndex(p => p.Priority)
            .HasDatabaseName("IX_Policies_Priority");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Policies_IsDeleted");

        builder.HasIndex(p => new { p.IsActive, p.IsDeleted, p.Priority })
            .HasDatabaseName("IX_Policies_IsActive_IsDeleted_Priority")
            .HasFilter("[IsActive] = 1 AND [IsDeleted] = 0"); // Optimizar búsqueda de políticas activas

        // Relaciones
        builder.HasMany(p => p.Conditions)
            .WithOne(pc => pc.Policy)
            .HasForeignKey(pc => pc.PolicyId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina la política, eliminar sus condiciones

        builder.HasMany(p => p.PolicyActions)
            .WithOne(pa => pa.Policy)
            .HasForeignKey(pa => pa.PolicyId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina la política, eliminar sus asociaciones

        builder.HasMany(p => p.AccessLogs)
            .WithOne(al => al.Policy)
            .HasForeignKey(al => al.PolicyId)
            .OnDelete(DeleteBehavior.SetNull); // Si se elimina la política, dejar null en logs (mantener histórico)
    }
}
