using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IProveedorPagoRepository : IRepositorioBase<ProveedorPago>
    {
        Task<ProveedorPago?> ObtenerPorIdAsync(int idEmpresa, int idProveedor, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProveedorPago>> ObtenerPorEmpresaAsync(int idEmpresa, CancellationToken cancellationToken = default);
    }
}
