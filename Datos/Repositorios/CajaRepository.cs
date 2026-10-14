using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class CajaRepository : RepositorioBase<Caja>, ICajaRepository
    {
        public CajaRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<Caja?> ObtenerPorIdAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdEmpresa == idEmpresa && c.IdCaja == idCaja, cancellationToken);
        }

        public async Task<IEnumerable<Caja>> ObtenerPorSucursalAsync(int idEmpresa, int idSucursal, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdEmpresa == idEmpresa && c.IdSucursal == idSucursal)
                .ToListAsync(cancellationToken);
        }
    }
}
