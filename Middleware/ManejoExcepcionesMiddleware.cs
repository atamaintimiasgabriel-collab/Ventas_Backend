using System.Net;
using System.Text.Json;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;

namespace SistemaVentasMotorrepuestos.API.Middleware
{
    // Intercepta excepciones no controladas globalmente y devuelve una respuesta estándar, segura y sin revelar información sensible (como stack trace).
    public class ManejoExcepcionesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ManejoExcepcionesMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ManejoExcepcionesMiddleware(RequestDelegate next, ILogger<ManejoExcepcionesMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Solo logueamos el mensaje técnico internamente, pero NO contraseñas ni JWTs
                _logger.LogError(ex, "Ocurrió una excepción no controlada en el request {TraceIdentifier}", context.TraceIdentifier);
                await ManejarExcepcionAsync(context, ex);
            }
        }

        private static bool EsFalloTransitorioDeBaseDeDatos(Exception exception)
        {
            for (var actual = exception; actual != null; actual = actual.InnerException)
            {
                // -2 = timeout, 53/-1 = servidor inaccesible, 40613/40197/40501 = no disponible, 1205 = deadlock
                if (actual is Microsoft.Data.SqlClient.SqlException sql &&
                    (sql.Number is -2 or 53 or -1 or 258 or 1205 or 40613 or 40197 or 40501 || sql.Class == 11))
                    return true;
                if (actual is TimeoutException) return true;
            }
            return false;
        }

        private Task ManejarExcepcionAsync(HttpContext context, Exception exception)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Ocurrió un error interno.";

            if (exception is PedidoNotFoundException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas.VentaNotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is PedidoDomainException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas.VentaDomainException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.TaxEngine.TaxEngineDomainException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas.FinanzasDomainException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
            }
            else if (exception is PedidoConcurrencyException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas.VentaConcurrencyException || exception is SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas.VentaIdempotencyException)
            {
                statusCode = (int)HttpStatusCode.Conflict;
                message = exception.Message;
            }
            else if (EsFalloTransitorioDeBaseDeDatos(exception))
            {
                // La base de datos no respondio a tiempo: se informa como servicio no disponible para que el cliente reintente.
                statusCode = (int)HttpStatusCode.ServiceUnavailable;
                message = "La base de datos no esta disponible temporalmente. Intenta nuevamente en unos segundos.";
                context.Response.Headers["Retry-After"] = "5";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = context.Response.StatusCode,
                message = message,
                traceId = context.TraceIdentifier
            };

            var payload = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(payload);
        }
    }
}
