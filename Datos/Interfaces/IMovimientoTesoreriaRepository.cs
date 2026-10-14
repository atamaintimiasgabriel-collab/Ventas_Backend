using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IMovimientoTesoreriaRepository : IRepositorioBase<MovimientoTesoreria>
    {
        Task<IEnumerable<MovimientoTesoreria>> ObtenerPorCuentaAsync(int idEmpresa, int idCuenta, CancellationToken cancellationToken = default);
    }
}
