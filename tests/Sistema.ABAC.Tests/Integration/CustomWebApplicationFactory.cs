using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Tests.Integration;

/// <summary>
/// Factory para pruebas de integración que reemplaza SQL Server por EF Core InMemory.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "TestingSecretKey!1234567890TestingSecretKey!");
        Environment.SetEnvironmentVariable("CONNECTION_STRING", "Server=(localdb)\\mssqllocaldb;Database=SistemaAbacTests;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=SistemaAbacTests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["JwtSettings:SecretKey"] = "TestingSecretKey!1234567890TestingSecretKey!",
                ["JwtSettings:Issuer"] = "Sistema.ABAC.Tests",
                ["JwtSettings:Audience"] = "Sistema.ABAC.Tests.Client",
                ["JwtSettings:ExpirationInMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationInDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AbacDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AbacDbContext>));
            services.RemoveAll(typeof(AbacDbContext));

            var databaseName = $"SistemaAbacTestDb-{Guid.NewGuid()}";
            services.AddDbContext<AbacDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });
    }
}
