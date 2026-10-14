using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface ITesoreriaRepository : IRepositorioBase<CuentaTesoreria>
    {
        Task<CuentaTesoreria?> ObtenerCuentaPorIdAsync(int idEmpresa, int idCuenta, CancellationToken cancellationToken = default);
        Task<IEnumerable<CuentaTesoreria>> ObtenerCuentasPorEmpresaAsync(int idEmpresa, CancellationToken cancellationToken = default);
    }
}
