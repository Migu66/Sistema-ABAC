using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Sistema.ABAC.Infrastructure.Persistence;

/// <summary>
/// DbContext principal del sistema ABAC.
/// Hereda de IdentityDbContext para integrar ASP.NET Core Identity.
/// </summary>
public class AbacDbContext : IdentityDbContext<IdentityUser>
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
