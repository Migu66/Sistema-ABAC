using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Sistema.ABAC.Application.Mappings;

/// <summary>
/// Clase de extensión para configurar AutoMapper en el contenedor de dependencias.
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    /// Registra los perfiles de AutoMapper en el contenedor de servicios.
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>La colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddApplicationAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            // Registrar todos los perfiles del ensamblado actual
            cfg.AddMaps(Assembly.GetExecutingAssembly());

            // Configuraciones globales de AutoMapper
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
        });

        return services;
    }
}
