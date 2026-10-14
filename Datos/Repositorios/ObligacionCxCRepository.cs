using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class ObligacionCxCRepository : RepositorioBase<ObligacionCxC>, IObligacionCxCRepository
    {
        public ObligacionCxCRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<ObligacionCxC?> ObtenerPorIdAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdEmpresa == idEmpresa && o.IdObligacion == idObligacion, cancellationToken);
        }

        public async Task<ObligacionCxC?> ObtenerPorVentaAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdEmpresa == idEmpresa && o.IdVentaOrigen == idVenta, cancellationToken);
        }

        public async Task<IEnumerable<ObligacionCxC>> ObtenerPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(o => o.IdEmpresa == idEmpresa && o.IdCliente == idCliente)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ObligacionCxC>> ObtenerPendientesPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(o => o.IdEmpresa == idEmpresa && o.IdCliente == idCliente && o.Estado == "PENDIENTE")
                .ToListAsync(cancellationToken);
        }
    }
}
