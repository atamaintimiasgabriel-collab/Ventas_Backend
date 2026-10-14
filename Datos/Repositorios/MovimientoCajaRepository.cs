using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class MovimientoCajaRepository : RepositorioBase<MovimientoCaja>, IMovimientoCajaRepository
    {
        public MovimientoCajaRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<IEnumerable<MovimientoCaja>> ObtenerPorAperturaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdEmpresa == idEmpresa && m.IdApertura == idAperturaCaja)
                .ToListAsync(cancellationToken);
        }
    }
}
