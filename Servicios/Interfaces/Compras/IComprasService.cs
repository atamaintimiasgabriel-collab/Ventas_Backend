using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.DTOs.Compras;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras
{
    public interface IComprasService
    {
        Task<IEnumerable<CompraDTO>> ObtenerComprasAsync(int idEmpresa, int idSucursal);
        Task<CompraDTO> RegistrarCompraAsync(int idEmpresa, int idSucursal, int idUsuario, NuevaCompraDTO request);
        Task<CompraDTO> CambiarEstadoCompraAsync(int idCompra, CambioEstadoCompraDTO request, int idUsuario);
        Task<CompraDTO?> ObtenerCompraPorIdAsync(int idCompra);
        Task<Dictionary<string, object>> ObtenerKpisAsync(int idEmpresa, int idSucursal);
    }
}
