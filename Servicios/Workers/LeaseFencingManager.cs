using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Workers
{
    public class LeaseFencingManager
    {
        private readonly ApplicationDbContext _dbContext;
        
        public LeaseFencingManager(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IntentarAdquirirLeaseAsync(
            Comprobante comprobante, 
            string workerId, 
            Guid executionToken, 
            TimeSpan leaseDuration, 
            CancellationToken cancellationToken = default)
        {
            var ahora = DateTime.UtcNow;
            var leaseHasta = ahora.Add(leaseDuration);

            // La elegibilidad y la adquisición ocurren en un único UPDATE.
            var filas = await _dbContext.Comprobantes
                .Where(c => c.IdComprobante == comprobante.IdComprobante &&
                            c.IdEmpresa == comprobante.IdEmpresa &&
                            (c.EstadoProcesamiento == "LIBRE" ||
                             (c.EstadoProcesamiento == "BLOQUEADO" &&
                              (c.LeaseHasta == null || c.LeaseHasta <= ahora))))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.EstadoProcesamiento, "BLOQUEADO")
                    .SetProperty(c => c.WorkerId, workerId)
                    .SetProperty(c => c.ExecutionToken, executionToken)
                    .SetProperty(c => c.LeaseHasta, leaseHasta)
                    .SetProperty(c => c.NumeroIntento, c => c.NumeroIntento + 1),
                    cancellationToken);

            if (filas != 1)
                return false;

            await _dbContext.Entry(comprobante).ReloadAsync(cancellationToken);
            return true;
        }

        public async Task LiberarLeaseAsync(
            Comprobante comprobante, 
            Guid executionToken, 
            string nuevoEstadoProcesamiento, 
            string nuevoEstadoFiscal, 
            string eventoAuditoria,
            string detalleAuditoria,
            DateTime? fechaProximoIntento = null,
            CancellationToken cancellationToken = default)
        {
            if (comprobante.ExecutionToken != executionToken)
            {
                throw new InvalidOperationException("Fencing Token mismatch. El worker ha perdido el lease.");
            }

            var ahora = DateTime.UtcNow;
            var rowVersion = comprobante.RowVersion.ToArray();
            var estadoAnteriorProc = comprobante.EstadoProcesamiento;
            var estadoAnteriorFisc = comprobante.EstadoFiscal;
            var idDocumentoExterno = comprobante.IdDocumentoExterno;
            var idOperacionExterna = comprobante.IdOperacionExterna;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // La autoridad y el efecto se evalúan en un único UPDATE. Un lease expirado,
                // token sustituido o RowVersion obsoleto produce cero filas afectadas.
                var filas = await _dbContext.Comprobantes
                    .Where(c => c.IdComprobante == comprobante.IdComprobante &&
                                c.IdEmpresa == comprobante.IdEmpresa &&
                                c.EstadoProcesamiento == "BLOQUEADO" &&
                                c.ExecutionToken == executionToken &&
                                c.LeaseHasta != null &&
                                c.LeaseHasta > ahora &&
                                c.RowVersion.SequenceEqual(rowVersion))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.EstadoProcesamiento, nuevoEstadoProcesamiento)
                        .SetProperty(c => c.EstadoFiscal, nuevoEstadoFiscal)
                        .SetProperty(c => c.WorkerId, (string?)null)
                        .SetProperty(c => c.ExecutionToken, (Guid?)null)
                        .SetProperty(c => c.LeaseHasta, (DateTime?)null)
                        .SetProperty(c => c.FechaProximoIntento, fechaProximoIntento)
                        .SetProperty(c => c.IdDocumentoExterno, idDocumentoExterno)
                        .SetProperty(c => c.IdOperacionExterna, idOperacionExterna),
                        cancellationToken);

                if (filas != 1)
                    throw new InvalidOperationException("El worker perdió la autoridad antes de persistir el resultado.");

                _dbContext.Entry(comprobante).State = EntityState.Detached;
                _dbContext.ComprobantesAuditoria.Add(new ComprobanteAuditoria
                {
                    IdEmpresa = comprobante.IdEmpresa,
                    IdComprobante = comprobante.IdComprobante,
                    EstadoProcesamientoAnterior = estadoAnteriorProc,
                    EstadoProcesamientoNuevo = nuevoEstadoProcesamiento,
                    EstadoFiscalAnterior = estadoAnteriorFisc,
                    EstadoFiscalNuevo = nuevoEstadoFiscal,
                    Evento = eventoAuditoria,
                    FechaEvento = ahora,
                    Detalle = detalleAuditoria,
                    ExecutionToken = executionToken
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                comprobante.EstadoProcesamiento = nuevoEstadoProcesamiento;
                comprobante.EstadoFiscal = nuevoEstadoFiscal;
                comprobante.WorkerId = null;
                comprobante.ExecutionToken = null;
                comprobante.LeaseHasta = null;
                comprobante.FechaProximoIntento = fechaProximoIntento;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.Entry(comprobante).State = EntityState.Detached;
                throw;
            }
        }
        
        public async Task AsignarCorrelativoAsync(
            Comprobante comprobante, 
            Guid executionToken, 
            CancellationToken cancellationToken = default)
        {
            if (comprobante.ExecutionToken != executionToken)
            {
                throw new InvalidOperationException("Fencing Token mismatch al asignar correlativo.");
            }

            if (comprobante.CorrelativoFiscal.HasValue)
            {
                return; // Ya tiene correlativo
            }

            await VerificarAutoridadAsync(comprobante, executionToken, cancellationToken);

            // Iniciar transacción de base de datos
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Leer la serie fiscal con bloqueo exclusivo o permitiendo EF manejar la concurrencia
                var serieFiscal = await _dbContext.SeriesFiscales
                    .FirstOrDefaultAsync(s => s.IdEmpresa == comprobante.IdEmpresa && 
                                              s.TipoComprobante == comprobante.TipoComprobante && 
                                              s.Serie == comprobante.SerieFiscal, cancellationToken);

                if (serieFiscal == null)
                    throw new InvalidOperationException($"Serie fiscal {comprobante.SerieFiscal} no encontrada para asignar correlativo.");

                serieFiscal.CorrelativoActual += 1;
                comprobante.CorrelativoFiscal = serieFiscal.CorrelativoActual;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task VerificarAutoridadAsync(
            Comprobante comprobante,
            Guid executionToken,
            CancellationToken cancellationToken = default)
        {
            if (comprobante.ExecutionToken != executionToken)
                throw new InvalidOperationException("Fencing Token mismatch. El worker ha perdido el lease.");

            var autoridad = await _dbContext.Comprobantes
                .AsNoTracking()
                .Where(c => c.IdComprobante == comprobante.IdComprobante &&
                            c.IdEmpresa == comprobante.IdEmpresa)
                .Select(c => new
                {
                    c.EstadoProcesamiento,
                    c.ExecutionToken,
                    c.LeaseHasta,
                    c.RowVersion
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (autoridad is null ||
                autoridad.EstadoProcesamiento != "BLOQUEADO" ||
                autoridad.ExecutionToken != executionToken ||
                autoridad.LeaseHasta is null ||
                autoridad.LeaseHasta <= DateTime.UtcNow ||
                !autoridad.RowVersion.SequenceEqual(comprobante.RowVersion))
            {
                throw new InvalidOperationException("El worker ha perdido la autoridad de fencing sobre el comprobante.");
            }
        }

        public async Task PersistirStorageKeyXmlAsync(
            Comprobante comprobante,
            Guid executionToken,
            string storageKeyXml,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKeyXml))
                throw new ArgumentException("StorageKeyXml es obligatoria.", nameof(storageKeyXml));

            if (comprobante.ExecutionToken != executionToken)
                throw new InvalidOperationException("Fencing Token mismatch. El worker ha perdido el lease.");

            var ahora = DateTime.UtcNow;
            var rowVersion = comprobante.RowVersion.ToArray();
            var filas = await _dbContext.Comprobantes
                .Where(c => c.IdComprobante == comprobante.IdComprobante &&
                            c.IdEmpresa == comprobante.IdEmpresa &&
                            c.EstadoProcesamiento == "BLOQUEADO" &&
                            c.ExecutionToken == executionToken &&
                            c.LeaseHasta != null &&
                            c.LeaseHasta > ahora &&
                            c.RowVersion.SequenceEqual(rowVersion))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.StorageKeyXml, storageKeyXml), cancellationToken);

            if (filas != 1)
            {
                _dbContext.Entry(comprobante).State = EntityState.Detached;
                throw new InvalidOperationException(
                    "El worker perdió la autoridad antes de persistir StorageKeyXml.");
            }

            await _dbContext.Entry(comprobante).ReloadAsync(cancellationToken);
        }
    }
}
