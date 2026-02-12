using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad UserAttribute.
/// Define el esquema de la tabla de atributos asignados a usuarios.
/// </summary>
public class UserAttributeConfiguration : IEntityTypeConfiguration<UserAttribute>
{
    public void Configure(EntityTypeBuilder<UserAttribute> builder)
    {
        // Nombre de la tabla
        builder.ToTable("UserAttributes");

        // Clave primaria
        builder.HasKey(ua => ua.Id);

        // Configuración de propiedades
        builder.Property(ua => ua.UserId)
            .IsRequired()
            .HasComment("Identificador del usuario al que se asigna el atributo");

        builder.Property(ua => ua.AttributeId)
            .IsRequired()
            .HasComment("Identificador del atributo que se asigna");

        builder.Property(ua => ua.Value)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Valor concreto del atributo para este usuario");

        builder.Property(ua => ua.ValidFrom)
            .HasComment("Fecha desde la cual este valor es válido (null = desde siempre)");

        builder.Property(ua => ua.ValidTo)
            .HasComment("Fecha hasta la cual este valor es válido (null = indefinidamente)");

        // Propiedades heredadas de BaseEntity
        builder.Property(ua => ua.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha de creación del registro");

        builder.Property(ua => ua.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(ua => ua.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices
        builder.HasIndex(ua => ua.UserId)
            .HasDatabaseName("IX_UserAttributes_UserId");

        builder.HasIndex(ua => ua.AttributeId)
            .HasDatabaseName("IX_UserAttributes_AttributeId");

        builder.HasIndex(ua => new { ua.UserId, ua.AttributeId })
            .HasDatabaseName("IX_UserAttributes_UserId_AttributeId");

        builder.HasIndex(ua => ua.IsDeleted)
            .HasDatabaseName("IX_UserAttributes_IsDeleted");

        builder.HasIndex(ua => new { ua.ValidFrom, ua.ValidTo })
            .HasDatabaseName("IX_UserAttributes_ValidFrom_ValidTo");

        // Relaciones
        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserAttributes)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el usuario, eliminar sus atributos

        builder.HasOne(ua => ua.Attribute)
            .WithMany(a => a.UserAttributes)
            .HasForeignKey(ua => ua.AttributeId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar atributo si está en uso
    }
}
