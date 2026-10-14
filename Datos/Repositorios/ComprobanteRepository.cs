using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class ComprobanteRepository : RepositorioBase<Comprobante>, IComprobanteRepository
    {
        public ComprobanteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Comprobante?> ObtenerPorIdAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComprobante == idComprobante && c.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<Comprobante?> ObtenerPorIdConAuditoriaAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Auditorias.OrderBy(a => a.FechaEvento))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdComprobante == idComprobante && c.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<IEnumerable<Comprobante>> ObtenerPorVentaAsync(int idVenta, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdVenta == idVenta && c.IdEmpresa == idEmpresa)
                .OrderBy(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<Comprobante?> ObtenerPorIdentidadFiscalAsync(int idEmpresa, string tipoComprobante, string serieFiscal, long correlativoFiscal, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdEmpresa == idEmpresa && c.TipoComprobante == tipoComprobante && c.SerieFiscal == serieFiscal && c.CorrelativoFiscal == correlativoFiscal, cancellationToken);
        }

        public async Task<IEnumerable<Comprobante>> ObtenerPorIdComprobanteOrigenAsync(Guid idComprobanteOrigen, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdComprobanteOrigen == idComprobanteOrigen && c.IdEmpresa == idEmpresa)
                .OrderBy(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comprobante>> ObtenerPendientesEmisionAsync(int idEmpresa, int limit, CancellationToken cancellationToken = default)
        {
            var ahora = DateTime.UtcNow;

            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdEmpresa == idEmpresa && c.EstadoFiscal == "NO_ENVIADO" && 
                            (c.EstadoProcesamiento == "LIBRE" || (c.EstadoProcesamiento == "BLOQUEADO" && c.LeaseHasta < ahora)) &&
                            c.FechaProximoIntento <= ahora)
                .OrderBy(c => c.FechaProximoIntento)
                .ThenBy(c => c.FechaCreacion)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comprobante>> ObtenerPendientesConsultaAsync(int idEmpresa, int limit, CancellationToken cancellationToken = default)
        {
            var ahora = DateTime.UtcNow;

            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdEmpresa == idEmpresa && c.EstadoFiscal == "ENVIADO_DESCONOCIDO" && 
                            (c.EstadoProcesamiento == "LIBRE" || (c.EstadoProcesamiento == "BLOQUEADO" && c.LeaseHasta < ahora)) &&
                            c.FechaProximoIntento <= ahora)
                .OrderBy(c => c.FechaProximoIntento)
                .ThenBy(c => c.FechaCreacion)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteComprobanteParaVentaAsync(int idVenta, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(c => c.IdVenta == idVenta && c.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<bool> ExisteIdentidadFiscalAsync(int idEmpresa, string tipoComprobante, string serieFiscal, long correlativoFiscal, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(c => c.IdEmpresa == idEmpresa && c.TipoComprobante == tipoComprobante && c.SerieFiscal == serieFiscal && c.CorrelativoFiscal == correlativoFiscal, cancellationToken);
        }
    }
}
