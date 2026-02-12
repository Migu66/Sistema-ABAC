using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Infrastructure.Persistence;

/// <summary>
/// Clase encargada de inicializar la base de datos con datos semilla (seed data).
/// Crea los registros básicos necesarios para que el sistema ABAC funcione correctamente.
/// </summary>
/// <remarks>
/// Esta clase se debe ejecutar una vez después de crear la base de datos con las migraciones.
/// Inserta datos que son fundamentales para el funcionamiento del sistema:
/// - Acciones básicas (CRUD y operaciones comunes)
/// - Atributos predefinidos (características que se usarán frecuentemente)
/// - Roles básicos del sistema
/// </remarks>
public class DbInitializer
{
    private readonly AbacDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DbInitializer(AbacDbContext context, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Inicializa la base de datos con todos los datos semilla.
    /// Verifica que no existan datos antes de insertarlos para evitar duplicados.
    /// </summary>
    /// <returns>True si se insertaron datos, False si ya existían.</returns>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            // Asegurar que la base de datos esté creada
            await _context.Database.MigrateAsync();

            // Verificar si ya hay datos iniciales
            if (await _context.Actions.AnyAsync() || 
                await _context.Attributes.AnyAsync() || 
                await _roleManager.Roles.AnyAsync())
            {
                // Los datos ya existen, no hacer nada
                return false;
            }

            // Insertar datos en orden lógico
            await SeedRolesAsync();
            await SeedActionsAsync();
            await SeedAttributesAsync();

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // En producción, aquí se debería usar un logger
            throw new Exception("Error al inicializar la base de datos con datos semilla", ex);
        }
    }

    /// <summary>
    /// Crea los roles básicos del sistema.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            "Administrador",
            "Usuario",
            "Auditor",
            "Supervisor"
        };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }
    }

    /// <summary>
    /// Crea las acciones básicas del sistema (operaciones CRUD y comunes).
    /// Estas son las acciones que se pueden permitir o denegar en las políticas ABAC.
    /// </summary>
    private async Task SeedActionsAsync()
    {
        var actions = new[]
        {
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Crear",
                Code = "create",
                Description = "Permite crear nuevos recursos en el sistema",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Leer",
                Code = "read",
                Description = "Permite visualizar y consultar recursos existentes",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Actualizar",
                Code = "update",
                Description = "Permite modificar recursos existentes",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Eliminar",
                Code = "delete",
                Description = "Permite borrar recursos del sistema",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Aprobar",
                Code = "approve",
                Description = "Permite aprobar solicitudes, documentos o recursos pendientes",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Rechazar",
                Code = "reject",
                Description = "Permite rechazar solicitudes, documentos o recursos pendientes",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Exportar",
                Code = "export",
                Description = "Permite exportar datos a formatos externos (Excel, PDF, CSV)",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Importar",
                Code = "import",
                Description = "Permite importar datos desde archivos externos",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Compartir",
                Code = "share",
                Description = "Permite compartir recursos con otros usuarios",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Action
            {
                Id = Guid.NewGuid(),
                Name = "Auditar",
                Code = "audit",
                Description = "Permite revisar logs de auditoría y accesos al sistema",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Actions.AddRangeAsync(actions);
    }

    /// <summary>
    /// Crea los atributos predefinidos que se usarán frecuentemente en el sistema.
    /// Estos atributos se pueden asignar a usuarios, recursos o usar en contextos de evaluación.
    /// </summary>
    private async Task SeedAttributesAsync()
    {
        var attributes = new[]
        {
            // ===== ATRIBUTOS DE USUARIO =====
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Departamento",
                Key = "departamento",
                Type = AttributeType.String,
                Description = "Departamento organizacional al que pertenece el usuario",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Nivel de Acceso",
                Key = "nivel_acceso",
                Type = AttributeType.Number,
                Description = "Nivel numérico de autorización del usuario (1-10). Mayor número = mayor acceso",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Rol Organizacional",
                Key = "rol_organizacional",
                Type = AttributeType.String,
                Description = "Rol del usuario en la organización (Gerente, Empleado, Contratista, etc.)",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Es Supervisor",
                Key = "es_supervisor",
                Type = AttributeType.Boolean,
                Description = "Indica si el usuario tiene capacidades de supervisión",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Fecha de Contratación",
                Key = "fecha_contratacion",
                Type = AttributeType.DateTime,
                Description = "Fecha en que el usuario fue contratado en la organización",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Ubicación",
                Key = "ubicacion",
                Type = AttributeType.String,
                Description = "Ubicación física o región geográfica del usuario",
                CreatedAt = DateTime.UtcNow
            },
            // ===== ATRIBUTOS DE RECURSO =====
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Clasificación de Seguridad",
                Key = "clasificacion_seguridad",
                Type = AttributeType.String,
                Description = "Nivel de confidencialidad del recurso (Público, Interno, Confidencial, Secreto)",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Propietario",
                Key = "propietario",
                Type = AttributeType.String,
                Description = "Usuario o departamento propietario del recurso",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Categoría",
                Key = "categoria",
                Type = AttributeType.String,
                Description = "Categoría o tipo del recurso para clasificación",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Requiere Aprobación",
                Key = "requiere_aprobacion",
                Type = AttributeType.Boolean,
                Description = "Indica si las operaciones sobre este recurso requieren aprobación adicional",
                CreatedAt = DateTime.UtcNow
            },
            // ===== ATRIBUTOS DE CONTEXTO/AMBIENTE =====
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Hora del Día",
                Key = "hora_dia",
                Type = AttributeType.Number,
                Description = "Hora actual del sistema (0-23) para políticas temporales",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Día de la Semana",
                Key = "dia_semana",
                Type = AttributeType.String,
                Description = "Día de la semana actual para restricciones temporales",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "Es Horario Laboral",
                Key = "es_horario_laboral",
                Type = AttributeType.Boolean,
                Description = "Indica si la solicitud se hace dentro del horario laboral",
                CreatedAt = DateTime.UtcNow
            },
            new Domain.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = "IP del Cliente",
                Key = "ip_cliente",
                Type = AttributeType.String,
                Description = "Dirección IP desde donde se origina la solicitud",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Attributes.AddRangeAsync(attributes);
    }

    /// <summary>
    /// Limpia todos los datos de la base de datos (uso solo para desarrollo/testing).
    /// ⚠️ PELIGRO: Esta operación borra TODOS los datos del sistema.
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.MigrateAsync();
    }
}
