using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Infrastructure.Persistence.Interceptors;

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
public class AbacDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IAbacDbContext
{
    public AbacDbContext(DbContextOptions<AbacDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configura el DbContext con opciones adicionales, incluyendo interceptores.
    /// </summary>
    /// <param name="optionsBuilder">Constructor de opciones del contexto.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Registrar el interceptor de auditoría automática
        // Este interceptor se ejecuta antes de cada SaveChanges/SaveChangesAsync
        // para actualizar automáticamente CreatedAt, UpdatedAt e IsDeleted
        optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());
    }

    // ============================================================
    // DbSets - Representan las tablas en la base de datos
    // ============================================================
    
    /// <summary>
    /// Tabla de atributos del sistema (características que se pueden asignar a usuarios o recursos).
    /// Ejemplo: "Departamento", "Nivel de Seguridad", "Ubicación".
    /// </summary>
    public DbSet<Domain.Entities.Attribute> Attributes { get; set; }

    /// <summary>
    /// Tabla de atributos asignados a usuarios con sus valores.
    /// Ejemplo: Usuario Juan tiene atributo "Departamento" con valor "IT".
    /// </summary>
    public DbSet<UserAttribute> UserAttributes { get; set; }

    /// <summary>
    /// Tabla de recursos protegidos por el sistema ABAC.
    /// Ejemplo: "Archivo Confidencial", "Base de Datos de Clientes".
    /// </summary>
    public DbSet<Resource> Resources { get; set; }

    /// <summary>
    /// Tabla de atributos asignados a recursos con sus valores.
    /// Ejemplo: Recurso "Archivo X" tiene atributo "Clasificación" con valor "Confidencial".
    /// </summary>
    public DbSet<ResourceAttribute> ResourceAttributes { get; set; }

    /// <summary>
    /// Tabla de acciones que se pueden realizar sobre recursos.
    /// Ejemplo: "Leer", "Escribir", "Eliminar".
    /// </summary>
    public DbSet<Domain.Entities.Action> Actions { get; set; }

    /// <summary>
    /// Tabla de políticas ABAC que definen las reglas de acceso.
    /// Ejemplo: "Permitir leer si el usuario tiene nivel >= recurso.nivelRequerido".
    /// </summary>
    public DbSet<Policy> Policies { get; set; }

    /// <summary>
    /// Tabla de condiciones que forman parte de una política.
    /// Ejemplo: "User.Department == 'IT' AND User.Level >= 5".
    /// </summary>
    public DbSet<PolicyCondition> PolicyConditions { get; set; }

    /// <summary>
    /// Tabla intermedia que relaciona políticas con las acciones a las que aplican.
    /// Ejemplo: Una política puede aplicar a las acciones "Leer" y "Escribir".
    /// </summary>
    public DbSet<PolicyAction> PolicyActions { get; set; }

    /// <summary>
    /// Tabla de auditoría que registra todos los intentos de acceso al sistema.
    /// Ejemplo: "Usuario Juan intentó leer Archivo X - Resultado: Permitido".
    /// </summary>
    public DbSet<AccessLog> AccessLogs { get; set; }

    /// <summary>
    /// Tabla de tokens de actualización (refresh tokens) para renovación de tokens JWT.
    /// Almacena los refresh tokens activos de cada usuario para validación y revocación.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones de entidades del ensamblado actual
        // Esto busca automáticamente todas las clases que implementan IEntityTypeConfiguration<T>
        // y aplica sus configuraciones (AttributeConfiguration, PolicyConfiguration, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AbacDbContext).Assembly);

        // ============================================================
        // GLOBAL QUERY FILTERS - Soft Delete
        // ============================================================
        // Los Global Query Filters aplican automáticamente condiciones WHERE
        // a TODAS las consultas de las entidades especificadas.
        // Esto implementa el patrón Soft Delete: los registros marcados como
        // IsDeleted = true nunca aparecen en consultas normales.
        
        // Aplicar filtro a todas las entidades que heredan de BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verificar si la entidad hereda de BaseEntity
            if (typeof(Domain.Common.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Construir la expresión del filtro: e => !e.IsDeleted
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(Domain.Common.BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Not(property),
                    parameter
                );

                // Aplicar el filtro a la entidad
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Aplicar filtro a User (que no hereda de BaseEntity pero tiene IsDeleted)
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        // NOTA: Para consultar registros eliminados intencionalmente, usar:
        // context.Entities.IgnoreQueryFilters().Where(e => e.IsDeleted).ToList()
    }
}
