using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.DTOs.Compras;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras
{
    public interface IProveedoresService
    {
        Task<ProveedorPaginadoDTO> ListarAsync(int idEmpresa, ProveedorFiltroDTO filtro);
        Task<ProveedorDTO?> ObtenerProveedorPorIdAsync(int idEmpresa, int idProveedor);
        Task<ProveedorDTO> CrearAsync(int idEmpresa, CrearProveedorDTO request);
        Task<ProveedorDTO> ActualizarAsync(int idEmpresa, int idProveedor, ActualizarProveedorDTO request);
        Task<ProveedorDTO> CambiarEstadoAsync(int idEmpresa, int idProveedor, bool activo);
        Task<IEnumerable<CompraDTO>> ObtenerComprasAsync(int idEmpresa, int idProveedor);
        Task<ProveedorEstadisticasDTO> ObtenerEstadisticasAsync(int idEmpresa);
        Task<IEnumerable<ProveedorDTO>> ObtenerProveedoresActivosAsync(int idEmpresa);
    }
}
