using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class PagoRepository : RepositorioBase<Pago>, IPagoRepository
    {
        public PagoRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<Pago?> ObtenerPorIdAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdEmpresa == idEmpresa && p.IdPago == idPago, cancellationToken);
        }

        public async Task<Pago?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdEmpresa == idEmpresa && p.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task<Pago?> BuscarPorReferenciaExternaAsync(int idEmpresa, int idProveedor, string referenciaExterna, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdEmpresa == idEmpresa && p.IdProveedor == idProveedor && p.ReferenciaExterna == referenciaExterna, cancellationToken);
        }

        public async Task<IEnumerable<Pago>> ObtenerPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.IdEmpresa == idEmpresa && p.IdCliente == idCliente)
                .ToListAsync(cancellationToken);
        }
    }
}
