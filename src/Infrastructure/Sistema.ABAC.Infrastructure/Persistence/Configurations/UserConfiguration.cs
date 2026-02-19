using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad User.
/// Define las propiedades y relaciones ABAC adicionales sobre la tabla AspNetUsers.
/// La configuración base de Identity (ToTable, índices de Identity, MaxLength de campos Identity)
/// se aplica en AbacDbContext.OnModelCreating para mantener separadas las responsabilidades.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Propiedades adicionales ABAC
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Nombre del usuario");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Apellido del usuario");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha y hora de creación del usuario");

        builder.Property(u => u.UpdatedAt)
            .HasComment("Fecha y hora de última actualización del usuario");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices adicionales
        builder.HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("IX_Users_FirstName_LastName");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        // Relaciones (ya configuradas desde el otro lado, pero las especificamos por claridad)
        builder.HasMany(u => u.UserAttributes)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina el usuario, eliminar sus atributos

        builder.HasMany(u => u.AccessLogs)
            .WithOne(al => al.User)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar usuarios con logs de auditoría
    }
}
