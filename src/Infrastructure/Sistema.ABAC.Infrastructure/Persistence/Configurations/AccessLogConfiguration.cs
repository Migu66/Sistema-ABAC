using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de Entity Framework para la entidad AccessLog.
/// Define el esquema de la tabla de auditoría de decisiones de acceso.
/// </summary>
public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        // Nombre de la tabla
        builder.ToTable("AccessLogs");

        // Clave primaria
        builder.HasKey(al => al.Id);

        // Configuración de propiedades
        builder.Property(al => al.UserId)
            .IsRequired()
            .HasComment("Identificador del usuario que intentó acceder");

        builder.Property(al => al.ResourceId)
            .HasComment("Identificador del recurso al que se intentó acceder (opcional)");

        builder.Property(al => al.ActionId)
            .HasComment("Identificador de la acción que se intentó realizar (opcional)");

        builder.Property(al => al.PolicyId)
            .HasComment("Identificador de la política que produjo la decisión (opcional)");

        builder.Property(al => al.Result)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Resultado de la evaluación (Permit, Deny, Error, NotApplicable)");

        builder.Property(al => al.Reason)
            .HasMaxLength(2000)
            .HasComment("Razón detallada de la decisión");

        builder.Property(al => al.Context)
            .HasColumnType("nvarchar(max)") // JSON largo
            .HasComment("Información contextual adicional en formato JSON");

        builder.Property(al => al.IpAddress)
            .HasMaxLength(45) // IPv6 puede tener hasta 45 caracteres
            .HasComment("Dirección IP desde la cual se realizó la solicitud");

        // Propiedades heredadas de BaseEntity
        builder.Property(al => al.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Fecha y hora del intento de acceso (timestamp)");

        builder.Property(al => al.UpdatedAt)
            .HasComment("Fecha de última actualización del registro");

        builder.Property(al => al.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de eliminación lógica (soft delete)");

        // Índices para búsquedas y reportes de auditoría
        builder.HasIndex(al => al.UserId)
            .HasDatabaseName("IX_AccessLogs_UserId");

        builder.HasIndex(al => al.ResourceId)
            .HasDatabaseName("IX_AccessLogs_ResourceId");

        builder.HasIndex(al => al.ActionId)
            .HasDatabaseName("IX_AccessLogs_ActionId");

        builder.HasIndex(al => al.PolicyId)
            .HasDatabaseName("IX_AccessLogs_PolicyId");

        builder.HasIndex(al => al.Result)
            .HasDatabaseName("IX_AccessLogs_Result");

        builder.HasIndex(al => al.CreatedAt)
            .HasDatabaseName("IX_AccessLogs_CreatedAt");

        builder.HasIndex(al => al.IpAddress)
            .HasDatabaseName("IX_AccessLogs_IpAddress");

        builder.HasIndex(al => al.IsDeleted)
            .HasDatabaseName("IX_AccessLogs_IsDeleted");

        // Índices compuestos para consultas comunes
        builder.HasIndex(al => new { al.UserId, al.CreatedAt })
            .HasDatabaseName("IX_AccessLogs_UserId_CreatedAt");

        builder.HasIndex(al => new { al.ResourceId, al.CreatedAt })
            .HasDatabaseName("IX_AccessLogs_ResourceId_CreatedAt");

        builder.HasIndex(al => new { al.Result, al.CreatedAt })
            .HasDatabaseName("IX_AccessLogs_Result_CreatedAt");

        // Relaciones
        builder.HasOne(al => al.User)
            .WithMany(u => u.AccessLogs)
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar usuarios con logs

        builder.HasOne(al => al.Resource)
            .WithMany(r => r.AccessLogs)
            .HasForeignKey(al => al.ResourceId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar recursos con logs

        builder.HasOne(al => al.Action)
            .WithMany(a => a.AccessLogs)
            .HasForeignKey(al => al.ActionId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar acciones con logs

        builder.HasOne(al => al.Policy)
            .WithMany(p => p.AccessLogs)
            .HasForeignKey(al => al.PolicyId)
            .OnDelete(DeleteBehavior.SetNull); // Si se elimina la política, dejar null (mantener histórico)
    }
}
