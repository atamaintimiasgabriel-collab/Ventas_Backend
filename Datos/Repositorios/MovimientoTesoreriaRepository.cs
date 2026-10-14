using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class MovimientoTesoreriaRepository : RepositorioBase<MovimientoTesoreria>, IMovimientoTesoreriaRepository
    {
        public MovimientoTesoreriaRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<IEnumerable<MovimientoTesoreria>> ObtenerPorCuentaAsync(int idEmpresa, int idCuenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdEmpresa == idEmpresa && m.IdCuenta == idCuenta)
                .ToListAsync(cancellationToken);
        }
    }
}
