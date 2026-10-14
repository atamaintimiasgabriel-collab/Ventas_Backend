using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class TesoreriaRepository : RepositorioBase<CuentaTesoreria>, ITesoreriaRepository
    {
        public TesoreriaRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<CuentaTesoreria?> ObtenerCuentaPorIdAsync(int idEmpresa, int idCuenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdEmpresa == idEmpresa && t.IdCuenta == idCuenta, cancellationToken);
        }

        public async Task<IEnumerable<CuentaTesoreria>> ObtenerCuentasPorEmpresaAsync(int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.IdEmpresa == idEmpresa)
                .ToListAsync(cancellationToken);
        }
    }
}
