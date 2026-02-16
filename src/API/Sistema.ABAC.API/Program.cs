using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using Serilog;
using Serilog.Events;
using Sistema.ABAC.API.Authorization;
using Sistema.ABAC.API.Middleware;
using Sistema.ABAC.Application.Mappings;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Repositories;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Infrastructure.Persistence;
using Sistema.ABAC.Infrastructure.Settings;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;

// Cargar variables de entorno desde archivo .env
DotNetEnv.Env.Load();

// Configurar Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando Sistema ABAC API");

    var builder = WebApplication.CreateBuilder(args);

    // Reemplazar el logging por defecto con Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithEnvironmentName()
    );

    // ============================================================
    // CONFIGURACIÓN DE SERVICIOS (Paso 5)
    // ============================================================

    // 1. Configurar DbContext con SQL Server
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
        ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

    builder.Services.AddDbContext<AbacDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        })
    );

    // 2. Configurar ASP.NET Core Identity con la entidad User personalizada
    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        // Configuración de contraseñas
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Configuración de lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Configuración de usuario
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        // Configuración de sign in
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AbacDbContext>()
    .AddDefaultTokenProviders();

    // 2.1 Registrar servicios ABAC y dependencias del handler de autorización
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IAttributeCollectorService, AttributeCollectorService>();
    builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
    builder.Services.AddScoped<IPolicyEvaluator, PolicyEvaluator>();
    builder.Services.AddScoped<IAccessControlService, AccessControlService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IAuthorizationHandler, AbacAuthorizationHandler>();

    // 3. Configurar JWT Settings
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

    // Obtener SecretKey desde variables de entorno si está disponible
    var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings.SecretKey;
    if (string.IsNullOrEmpty(jwtSecretKey))
    {
        throw new InvalidOperationException("JWT SecretKey no está configurado en appsettings.json o variables de entorno.");
    }

    builder.Services.Configure<JwtSettings>(options =>
    {
        options.SecretKey = jwtSecretKey;
        options.Issuer = jwtSettings.Issuer;
        options.Audience = jwtSettings.Audience;
        options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
        options.RefreshTokenExpirationInDays = jwtSettings.RefreshTokenExpirationInDays;
    });

    // 4. Configurar Autenticación JWT
    var key = Encoding.UTF8.GetBytes(jwtSecretKey);
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true; // En producción debe ser true
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Eliminar delay predeterminado de 5 minutos
        };

        // Configuración para eventos de autenticación
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Autenticación JWT falló: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("Token JWT validado correctamente para usuario: {User}",
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

    // 5. Configurar Autorización
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AbacAuthorizeAttribute.PolicyName,
            policy => policy.RequireAuthenticatedUser()
                            .AddRequirements(new AbacRequirement()));
    });

    // 6. Configurar CORS
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // En desarrollo, permitir cualquier origen
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                // En producción, restringir a orígenes específicos
                if (allowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException("No se han configurado orígenes CORS permitidos para producción.");
                }

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // 7. Agregar controladores
    builder.Services.AddControllers();

    // 7.1 Configurar Rate Limiting por IP
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // 8. Configurar FluentValidation
    builder.Services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<Sistema.ABAC.Application.DTOs.Auth.RegisterDto>();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://httpstatuses.com/400",
                Title = "Error de Validación",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Se han producido uno o más errores de validación. Revisa los detalles.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

    Log.Information("FluentValidation configurado correctamente");

    // 9. Configurar AutoMapper
    builder.Services.AddApplicationAutoMapper();
    Log.Information("AutoMapper configurado correctamente");

    // 10. Configurar Swagger/OpenAPI con documentación XML
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Incluir archivos XML de documentación
        var xmlFiles = new[]
        {
            "Sistema.ABAC.API.xml",
            "Sistema.ABAC.Application.xml",
            "Sistema.ABAC.Domain.xml"
        };

        foreach (var xmlFile in xmlFiles)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        }
    });

    Log.Information("Servicios configurados correctamente");

    // ============================================================
    // CONFIGURACIÓN DEL PIPELINE DE MIDDLEWARE (Paso 6)
    // ============================================================

    var app = builder.Build();

    // 1. Manejo de excepciones global (DEBE IR PRIMERO)
    app.UseExceptionHandling();

    // 2. Headers de seguridad
    app.UseSecurityHeaders();

    // 3. Logging de requests HTTP con Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

    // 4. Habilitar Swagger en desarrollo
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema ABAC API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Sistema ABAC API - Documentación";
            options.DisplayRequestDuration();
        });
    }

    // 5. Redirección HTTPS
    app.UseHttpsRedirection();

    // 6. Habilitar CORS (DEBE IR ANTES de Authentication/Authorization)
    app.UseCors();

    // 6.1 Habilitar Rate Limiting por IP
    app.UseIpRateLimiting();

    // 7. Autenticación (DEBE IR ANTES de Authorization)
    app.UseAuthentication();
    
    // 8. Autorización
    app.UseAuthorization();

    // 9.p.UseAuthorization();

    // Mapear controladores
    app.MapControllers();

    // Endpoint de prueba (opcional, se puede eliminar más adelante)
    app.MapGet("/health", () => new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    })
    .WithName("HealthCheck")
    .WithTags("Monitoring");

    Log.Information("Sistema ABAC API iniciado correctamente en {Environment}", app.Environment.EnvironmentName);
    Log.Information("Base de datos configurada: {ConnectionString}",
        connectionString.Contains("Password") ? "***CONNECTION_STRING_OCULTO***" : connectionString);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

