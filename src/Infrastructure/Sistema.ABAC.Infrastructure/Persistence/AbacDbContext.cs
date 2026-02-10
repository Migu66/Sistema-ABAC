using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;

namespace Sistema.ABAC.Infrastructure.Persistence;

/// <summary>
/// DbContext principal del sistema ABAC.
/// Hereda de IdentityDbContext para integrar ASP.NET Core Identity con la entidad User personalizada.
/// </summary>
/// <remarks>
/// <para>
/// Se utiliza IdentityDbContext&lt;User, IdentityRole&lt;Guid&gt;, Guid&gt; para:
/// - User: La entidad personalizada de usuario del dominio que hereda de IdentityUser&lt;Guid&gt;
/// - IdentityRole&lt;Guid&gt;: Los roles de Identity usando Guid como clave
/// - Guid: El tipo de dato para las claves primarias de las tablas de Identity
/// </para>
/// </remarks>
public class AbacDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AbacDbContext(DbContextOptions<AbacDbContext> options) : base(options)
    {
    }

    // DbSets para las entidades del dominio se agregarán en fases posteriores
    // public DbSet<Policy> Policies { get; set; }
    // public DbSet<Resource> Resources { get; set; }
    // etc...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de entidades se agregarán en Fase 3
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AbacDbContext).Assembly);

        // Global Query Filters para Soft Delete se configurarán más adelante
    }
}
