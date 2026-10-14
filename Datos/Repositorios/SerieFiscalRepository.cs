using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class SerieFiscalRepository : RepositorioBase<SerieFiscal>, ISerieFiscalRepository
    {
        public SerieFiscalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SerieFiscal?> ObtenerPorIdAsync(int idSerieFiscal, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSerieFiscal == idSerieFiscal && s.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<IEnumerable<SerieFiscal>> BuscarPorEmpresaAsync(int idEmpresa, int? idSucursal = null, bool? activo = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(s => s.IdEmpresa == idEmpresa);

            if (idSucursal.HasValue)
                query = query.Where(s => s.IdSucursal == idSucursal.Value);

            if (activo.HasValue)
                query = query.Where(s => s.Activo == activo.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<SerieFiscal?> BuscarSerieAsync(int idEmpresa, string tipoComprobante, string serie, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdEmpresa == idEmpresa && s.TipoComprobante == tipoComprobante && s.Serie == serie, cancellationToken);
        }

        public async Task<SerieFiscal?> ObtenerSerieActivaAsync(int idEmpresa, int idSucursal, string tipoComprobante, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.IdEmpresa == idEmpresa && s.IdSucursal == idSucursal && s.TipoComprobante == tipoComprobante && s.Activo)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExisteSerieAsync(int idEmpresa, string tipoComprobante, string serie, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(s => s.IdEmpresa == idEmpresa && s.TipoComprobante == tipoComprobante && s.Serie == serie, cancellationToken);
        }
    }
}
