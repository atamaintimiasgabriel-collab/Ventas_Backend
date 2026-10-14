using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IPedidoRepository : IRepositorioBase<Pedido>
    {
        Task<Pedido?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken = default);
        Task<Pedido?> ObtenerPedidoConDetallesAsync(int idPedido, int idEmpresa, CancellationToken cancellationToken = default);
    }
}
