namespace SistemaVentasMotorrepuestos.API.Middleware
{
    // Inyecta cabeceras de seguridad estrictas en las respuestas HTTP
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Evita MIME-sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            // Evita clickjacking (aunque en APIs no hay UI, protege los JSON ante navegadores legacy)
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            // Activa protección XSS en navegadores viejos
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            // No almacenar endpoints críticos (Login) para prevenir fuga accidental de JWT o credenciales.
            if (context.Request.Path.StartsWithSegments("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
                context.Response.Headers.Append("Pragma", "no-cache");
            }

            await _next(context);
        }
    }
}
