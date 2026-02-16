using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Tests.Integration;

/// <summary>
/// Factory para pruebas de integración que reemplaza SQL Server por EF Core InMemory.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AbacDbContext>));
            services.RemoveAll(typeof(AbacDbContext));

            var databaseName = $"SistemaAbacTestDb-{Guid.NewGuid()}";
            services.AddDbContext<AbacDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });
    }
}
