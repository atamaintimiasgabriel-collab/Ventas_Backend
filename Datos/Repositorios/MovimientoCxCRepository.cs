using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class MovimientoCxCRepository : RepositorioBase<MovimientoCxC>, IMovimientoCxCRepository
    {
        public MovimientoCxCRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<IEnumerable<MovimientoCxC>> ObtenerPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdEmpresa == idEmpresa && m.IdObligacion == idObligacion)
                .ToListAsync(cancellationToken);
        }
    }
}
