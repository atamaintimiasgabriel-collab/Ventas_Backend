using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class AplicacionPagoRepository : RepositorioBase<AplicacionPago>, IAplicacionPagoRepository
    {
        public AplicacionPagoRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<IEnumerable<AplicacionPago>> ObtenerPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdPago == idPago)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AplicacionPago>> ObtenerPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdObligacion == idObligacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> ObtenerMontoAplicadoPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdPago == idPago)
                .SumAsync(a => a.MontoAplicado, cancellationToken);
        }

        public async Task<decimal> ObtenerMontoAplicadoPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdObligacion == idObligacion)
                .SumAsync(a => a.MontoAplicado, cancellationToken);
        }
    }
}
