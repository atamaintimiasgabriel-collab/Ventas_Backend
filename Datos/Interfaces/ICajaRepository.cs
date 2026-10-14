using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface ICajaRepository : IRepositorioBase<Caja>
    {
        Task<Caja?> ObtenerPorIdAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken = default);
        Task<IEnumerable<Caja>> ObtenerPorSucursalAsync(int idEmpresa, int idSucursal, CancellationToken cancellationToken = default);
    }
}
