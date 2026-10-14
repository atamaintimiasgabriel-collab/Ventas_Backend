using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class ProveedorPagoRepository : RepositorioBase<ProveedorPago>, IProveedorPagoRepository
    {
        public ProveedorPagoRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<ProveedorPago?> ObtenerPorIdAsync(int idEmpresa, int idProveedor, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdEmpresa == idEmpresa && p.IdProveedor == idProveedor, cancellationToken);
        }

        public async Task<IEnumerable<ProveedorPago>> ObtenerPorEmpresaAsync(int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.IdEmpresa == idEmpresa)
                .ToListAsync(cancellationToken);
        }
    }
}
