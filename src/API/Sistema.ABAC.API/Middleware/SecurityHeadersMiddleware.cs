namespace Sistema.ABAC.API.Middleware;

/// <summary>
/// Middleware para agregar headers de seguridad HTTP estándar.
/// Protege contra ataques comunes como XSS, Clickjacking, MIME sniffing, etc.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // X-Content-Type-Options: Previene MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options: Protege contra clickjacking
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // X-XSS-Protection: Habilita protección XSS del navegador (legacy)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer-Policy: Controla información enviada en el header Referer
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content-Security-Policy: Previene XSS y otros ataques de inyección
        // Para APIs REST, usamos una política restrictiva básica
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

        // Permissions-Policy: Controla qué características del navegador se permiten
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // Strict-Transport-Security: Fuerza HTTPS (solo en producción)
        if (!context.Request.IsLocal())
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        await _next(context);
    }
}

/// <summary>
/// Extensiones para registrar el middleware de headers de seguridad.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static bool IsLocal(this HttpRequest request)
    {
        var connection = request.HttpContext.Connection;
        if (connection.RemoteIpAddress != null)
        {
            return connection.LocalIpAddress != null
                ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                : System.Net.IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        return true;
    }
}
