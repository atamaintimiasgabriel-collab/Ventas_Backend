using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SistemaVentasMotorrepuestos.API.Servicios.Workers
{
    public class FacturacionConsultorWorker : BackgroundService
    {
        private readonly ILogger<FacturacionConsultorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _workerId;

        public FacturacionConsultorWorker(ILogger<FacturacionConsultorWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _workerId = $"Consultor-{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FacturacionConsultorWorker iniciado con WorkerId: {WorkerId}", _workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarLoteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico en FacturacionConsultorWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Polling delay
            }
        }

        private async Task ProcesarLoteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var comprobanteRepo = scope.ServiceProvider.GetRequiredService<IComprobanteRepository>();
            var leaseManager = scope.ServiceProvider.GetRequiredService<LeaseFencingManager>();
            var proveedor = scope.ServiceProvider.GetRequiredService<IProveedorFacturacionElectronica>();
            var context = scope.ServiceProvider.GetRequiredService<SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext>();

            var empresasIds = context.Empresas.Where(e => e.Activo).Select(e => e.IdEmpresa).ToList();

            foreach (var idEmpresa in empresasIds)
            {
                var pendientes = await comprobanteRepo.ObtenerPendientesConsultaAsync(idEmpresa, 10, stoppingToken);

                foreach (var comprobante in pendientes)
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    var executionToken = Guid.NewGuid();
                    var leaseAdquirido = await leaseManager.IntentarAdquirirLeaseAsync(comprobante, _workerId, executionToken, TimeSpan.FromMinutes(2), stoppingToken);

                    if (!leaseAdquirido)
                    {
                        _logger.LogInformation("No se pudo adquirir lease para consultar Comprobante {IdComprobante}.", comprobante.IdComprobante);
                        continue;
                    }

                    try
                    {
                        // 1.5. Obtener Venta
                        var contextDb = scope.ServiceProvider.GetRequiredService<SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext>();
                        var venta = await contextDb.Ventas
                            .Include(v => v.Cliente)
                                .ThenInclude(c => c.Direcciones)
                            .Include(v => v.VentaDetalles)
                                .ThenInclude(d => d.Producto)
                            .FirstOrDefaultAsync(v => v.IdVenta == comprobante.IdVenta, stoppingToken);

                        if (venta == null)
                        {
                            throw new InvalidOperationException($"Venta {comprobante.IdVenta} no encontrada para comprobante {comprobante.IdComprobante}");
                        }

                        // 1.6. Construir DTO
                        var docBuilder = scope.ServiceProvider.GetRequiredService<IDocumentBuilder>();
                        var documento = await docBuilder.ConstruirDesdeVentaAsync(venta, comprobante, stoppingToken);

                        // Invocar proveedor para consultar estado
                        var respuesta = await proveedor.ConsultarEstadoAsync(documento, stoppingToken);

                        // Interpretar respuesta
                        string nuevoEstadoFiscal = respuesta.EstadoFiscal;
                        string nuevoEstadoProc = respuesta.EstadoFiscal == "ACEPTADO" || respuesta.EstadoFiscal == "RECHAZADO" || respuesta.EstadoFiscal == "OBSERVADO" 
                            ? "FINALIZADO" 
                            : "LIBRE";

                        DateTime? proximoIntento = nuevoEstadoProc == "LIBRE" ? DateTime.UtcNow.AddMinutes(5) : null;

                        await leaseManager.LiberarLeaseAsync(
                            comprobante, 
                            executionToken, 
                            nuevoEstadoProc, 
                            nuevoEstadoFiscal, 
                            "INTENTO_CONSULTA", 
                            respuesta.MensajeError ?? "Consulta procesada",
                            proximoIntento,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al consultar Comprobante {IdComprobante}", comprobante.IdComprobante);
                        
                        try
                        {
                            await leaseManager.LiberarLeaseAsync(
                                comprobante, 
                                executionToken, 
                                "LIBRE", 
                                "ENVIADO_DESCONOCIDO", 
                                "INTENTO_CONSULTA_FALLIDO", 
                                "Error inesperado durante consulta: " + ex.Message,
                                DateTime.UtcNow.AddMinutes(5),
                                stoppingToken);
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogError(innerEx, "Error al liberar lease tras fallo de consulta.");
                        }
                    }
                }
            }
        }
    }
}
