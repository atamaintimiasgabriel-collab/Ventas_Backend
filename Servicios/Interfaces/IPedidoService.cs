using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponse> CrearPedidoAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default);
        
        Task<PedidoResponse> ObtenerPedidoPorIdAsync(int idPedido, CancellationToken cancellationToken = default);
        
        // PagedResult asumido como una clase común en el proyecto. 
        // Si no existe, se cambiará por IEnumerable o lo que use el proyecto.
        // Dado que solo diseñamos, usaremos un retorno compatible.
        Task<IEnumerable<PedidoListResponse>> ListarPedidosAsync(PedidoFilterRequest filtro, CancellationToken cancellationToken = default);
        
        Task CambiarEstadoPedidoAsync(int idPedido, string nuevoEstado, byte[] rowVersion, CancellationToken cancellationToken = default);
        
        Task CancelarPedidoAsync(int idPedido, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default);
        
        Task RechazarPedidoAsync(int idPedido, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default);
    }
}
