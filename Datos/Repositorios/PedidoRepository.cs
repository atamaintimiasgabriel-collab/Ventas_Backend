using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class PedidoRepository : RepositorioBase<Pedido>, IPedidoRepository
    {
        public PedidoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pedido?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.IdEmpresa == idEmpresa && p.IdempotencyKey == idempotencyKey, cancellationToken);
        }

        public async Task<Pedido?> ObtenerPedidoConDetallesAsync(int idPedido, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido && p.IdEmpresa == idEmpresa, cancellationToken);
        }
    }
}
