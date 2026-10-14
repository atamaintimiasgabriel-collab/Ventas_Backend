using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IMovimientoCajaRepository : IRepositorioBase<MovimientoCaja>
    {
        Task<IEnumerable<MovimientoCaja>> ObtenerPorAperturaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken = default);
    }
}
