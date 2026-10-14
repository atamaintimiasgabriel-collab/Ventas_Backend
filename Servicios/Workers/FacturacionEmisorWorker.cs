using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Workers
{
    public class FacturacionEmisorWorker : BackgroundService
    {
        private readonly ILogger<FacturacionEmisorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _workerId;

        public FacturacionEmisorWorker(ILogger<FacturacionEmisorWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _workerId = $"Emisor-{Environment.MachineName}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FacturacionEmisorWorker iniciado con WorkerId: {WorkerId}", _workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarLoteAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico en FacturacionEmisorWorker");
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

            // Obtener empresas (idealmente desde un config o repo, para pruebas usamos un rango grande)
            // Aquí simplificamos consultando todas las empresas, pero en un diseño real se podría paralelizar por empresa.
            // Para simplificar, obtenemos los pendientes asumiendo IdEmpresa 1 (o iterando sobre empresas).
            var context = scope.ServiceProvider.GetRequiredService<SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext>();
            var empresasIds = await context.Comprobantes
                .Where(c => c.EstadoFiscal == "NO_ENVIADO" && 
                            (c.EstadoProcesamiento == "LIBRE" || (c.EstadoProcesamiento == "BLOQUEADO" && c.LeaseHasta < DateTime.UtcNow)))
                .Select(c => c.IdEmpresa)
                .Distinct()
                .ToListAsync(stoppingToken);

            foreach (var idEmpresa in empresasIds)
            {
                var pendientes = await comprobanteRepo.ObtenerPendientesEmisionAsync(idEmpresa, 10, stoppingToken);

                foreach (var comprobante in pendientes)
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    var executionToken = Guid.NewGuid();
                    var leaseAdquirido = await leaseManager.IntentarAdquirirLeaseAsync(comprobante, _workerId, executionToken, TimeSpan.FromMinutes(2), stoppingToken);

                    if (!leaseAdquirido)
                    {
                        _logger.LogInformation("No se pudo adquirir lease para Comprobante {IdComprobante}. Posible concurrencia.", comprobante.IdComprobante);
                        continue;
                    }

                    var proveedorInvocado = false;
                    try
                    {
                        // 1. Asignar Correlativo si no tiene
                        await leaseManager.AsignarCorrelativoAsync(comprobante, executionToken, stoppingToken);

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

                        var empresa = await contextDb.Empresas
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                e => e.IdEmpresa == comprobante.IdEmpresa && e.Activo,
                                stoppingToken);

                        if (empresa == null)
                        {
                            throw new InvalidOperationException(
                                $"Empresa {comprobante.IdEmpresa} no encontrada o inactiva para comprobante {comprobante.IdComprobante}");
                        }

                        // 1.6. Construir DTO
                        var docBuilder = scope.ServiceProvider.GetRequiredService<IDocumentBuilder>();
                        var documento = await docBuilder.ConstruirDesdeVentaAsync(venta, comprobante, empresa, stoppingToken);

                        // 1.7. Asegurar XML y persistir su StorageKey bajo fencing vigente
                        await leaseManager.VerificarAutoridadAsync(comprobante, executionToken, stoppingToken);
                        var artifactService = scope.ServiceProvider.GetRequiredService<IComprobanteArtifactService>();
                        var storageKeyXml = await artifactService.AsegurarXmlAsync(
                            documento,
                            comprobante.StorageKeyXml,
                            stoppingToken);
                        await leaseManager.VerificarAutoridadAsync(comprobante, executionToken, stoppingToken);
                        await leaseManager.PersistirStorageKeyXmlAsync(
                            comprobante,
                            executionToken,
                            storageKeyXml,
                            stoppingToken);

                        // 2. Invocar proveedor
                        await leaseManager.VerificarAutoridadAsync(comprobante, executionToken, stoppingToken);
                        proveedorInvocado = true;
                        var respuesta = await proveedor.EmitirAsync(documento, stoppingToken);

                        // 3. Interpretar respuesta
                        string nuevoEstadoFiscal = respuesta.EstadoFiscal;
                        string nuevoEstadoProc = respuesta.EstadoFiscal == "ACEPTADO" || respuesta.EstadoFiscal == "RECHAZADO" || respuesta.EstadoFiscal == "OBSERVADO" 
                            ? "FINALIZADO" 
                            : "LIBRE";

                        DateTime? proximoIntento = nuevoEstadoProc == "LIBRE" ? DateTime.UtcNow.AddMinutes(5) : null;

                        comprobante.IdDocumentoExterno = respuesta.IdDocumentoExterno ?? comprobante.IdDocumentoExterno;
                        comprobante.IdOperacionExterna = respuesta.IdOperacionExterna ?? comprobante.IdOperacionExterna;

                        await leaseManager.VerificarAutoridadAsync(comprobante, executionToken, stoppingToken);
                        await leaseManager.LiberarLeaseAsync(
                            comprobante, 
                            executionToken, 
                            nuevoEstadoProc, 
                            nuevoEstadoFiscal, 
                            "INTENTO_EMISION", 
                            respuesta.MensajeError ?? "Emisión procesada",
                            proximoIntento,
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar Comprobante {IdComprobante}", comprobante.IdComprobante);
                        
                        try
                        {
                            var estadoFiscal = proveedorInvocado ? "ENVIADO_DESCONOCIDO" : "NO_ENVIADO";
                            var evento = proveedorInvocado ? "INTENTO_EMISION_FALLIDO" : "PRE_EMISION_FALLIDA";
                            var detalle = proveedorInvocado
                                ? "Error inesperado durante emisión: " + ex.Message
                                : "Error antes de invocar al proveedor: " + ex.Message;

                            await leaseManager.LiberarLeaseAsync(
                                comprobante, 
                                executionToken, 
                                "LIBRE", 
                                estadoFiscal,
                                evento,
                                detalle,
                                DateTime.UtcNow.AddMinutes(5),
                                stoppingToken);
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogError(innerEx, "Error al liberar lease tras fallo de procesamiento.");
                        }
                    }
                }
            }
        }
    }
}
