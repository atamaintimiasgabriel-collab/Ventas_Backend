using SistemaVentasMotorrepuestos.API.DTOs.Ventas;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IVentaService
    {
        Task<VentaResponse> CrearVentaAsync(CrearVentaRequest request, CancellationToken cancellationToken = default);
        Task<VentaResponse> ObtenerVentaPorIdAsync(int idVenta, CancellationToken cancellationToken = default);
        Task<VentaResponse> ObtenerVentaPorNumeroAsync(string numeroVenta, CancellationToken cancellationToken = default);
        Task<VentaResponse> ObtenerVentaPorPedidoAsync(int idPedido, CancellationToken cancellationToken = default);
        Task<IEnumerable<VentaListResponse>> ListarVentasAsync(VentaFilterRequest filtro, CancellationToken cancellationToken = default);
        Task AnularVentaAsync(int idVenta, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default);
    }
}
