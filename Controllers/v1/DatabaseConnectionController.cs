using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using System.Diagnostics;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1;

/// <summary>
/// Expone una comprobacion puntual de conectividad con la base de datos.
/// </summary>
[ApiController]
[Route("api/v1/database")]
[AllowAnonymous]
public sealed class DatabaseConnectionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DatabaseConnectionController> _logger;
    private readonly IWebHostEnvironment _environment;

    public DatabaseConnectionController(
        ApplicationDbContext db,
        ILogger<DatabaseConnectionController> logger,
        IWebHostEnvironment environment)
    {
        _db = db;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Verifica que el proveedor de EF Core pueda abrir una conexion a la base de datos configurada.
    /// </summary>
    [HttpGet("connection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckConnection(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var connection = _db.Database.GetDbConnection();
        var checkedAtUtc = DateTime.UtcNow;

        try
        {
            await connection.OpenAsync(cancellationToken);
            stopwatch.Stop();

            return Ok(new
            {
                connected = true,
                status = "healthy",
                mensaje = "La conexión con la base de datos funciona correctamente.",
                server = connection.DataSource,
                database = connection.Database,
                provider = connection.GetType().Name,
                state = connection.State.ToString(),
                latencyMs = stopwatch.ElapsedMilliseconds,
                checkedAtUtc,
                servidor = connection.DataSource,
                baseDatos = connection.Database,
                proveedor = connection.GetType().Name,
                estadoConexion = "Abierta",
                tiempoRespuestaMs = stopwatch.ElapsedMilliseconds,
                entorno = _environment.EnvironmentName,
                fechaComprobacionUtc = checkedAtUtc,
                recomendacion = "No se requiere ninguna acción."
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(exception, "No fue posible abrir la conexion a la base de datos.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                connected = false,
                status = "unhealthy",
                mensaje = "No se pudo establecer conexión con la base de datos.",
                server = connection.DataSource,
                database = connection.Database,
                provider = connection.GetType().Name,
                state = connection.State.ToString(),
                latencyMs = stopwatch.ElapsedMilliseconds,
                checkedAtUtc,
                servidor = connection.DataSource,
                baseDatos = connection.Database,
                proveedor = connection.GetType().Name,
                estadoConexion = "Cerrada",
                tiempoRespuestaMs = stopwatch.ElapsedMilliseconds,
                entorno = _environment.EnvironmentName,
                fechaComprobacionUtc = checkedAtUtc,
                error = "No fue posible abrir la conexión. Revise la red, las credenciales y el firewall de la base de datos.",
                recomendacion = "Verifique el servidor, el puerto 1433, las credenciales y las reglas de firewall de Azure SQL."
            });
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
