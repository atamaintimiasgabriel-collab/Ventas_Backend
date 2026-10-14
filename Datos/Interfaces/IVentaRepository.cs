using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IVentaRepository : IRepositorioBase<Venta>
    {
        Task<Venta?> ObtenerVentaPorIdAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default);
        Task<Venta?> ObtenerVentaConDetallesAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default);
        Task<Venta?> ObtenerVentaPorNumeroAsync(int idEmpresa, string numeroVenta, CancellationToken cancellationToken = default);
        Task<Venta?> ObtenerVentaPorPedidoAsync(int idEmpresa, int idPedido, CancellationToken cancellationToken = default);
        Task<bool> ExisteVentaParaPedidoAsync(int idEmpresa, int idPedido, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Venta> Ventas, int TotalRecords)> ObtenerVentasPaginadasAsync(
            int idEmpresa, 
            int pageNumber, 
            int pageSize,
            string? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            int? idCliente = null,
            string? origen = null,
            CancellationToken cancellationToken = default);
    }
}
