using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad RefreshToken.
/// Define el esquema de la tabla de tokens de actualización para renovación de JWT.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Nombre de la tabla
        builder.ToTable("RefreshTokens");

        // Clave primaria
        builder.HasKey(rt => rt.Id);

        // Configuración de propiedades
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Token de actualización único");

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasComment("Identificador del usuario propietario del token");

        builder.Property(rt => rt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha y hora de creación del token");

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired()
            .HasComment("Fecha y hora de expiración del token");

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indica si el token ha sido revocado manualmente");

        builder.Property(rt => rt.RevokedAt)
            .HasComment("Fecha y hora en que se revocó el token");

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45) // IPv6 puede tener hasta 45 caracteres
            .HasComment("Dirección IP desde la cual se creó el token");

        builder.Property(rt => rt.ReplacedByTokenId)
            .HasComment("ID del token que reemplazó a este cuando se renovó");

        // Índices para búsquedas rápidas
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        // Relación con User
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade) // Si se elimina el usuario, se eliminan sus tokens
            .HasConstraintName("FK_RefreshTokens_Users_UserId");
    }
}
