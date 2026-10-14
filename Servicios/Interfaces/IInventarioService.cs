using SistemaVentasMotorrepuestos.API.DTOs.Inventario;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IInventarioService
    {
        Task DescontarStockPorVentaAsync(int idEmpresa, int idVenta, string origen, IEnumerable<DescuentoStockRequest> items, CancellationToken cancellationToken = default);
    }
}
