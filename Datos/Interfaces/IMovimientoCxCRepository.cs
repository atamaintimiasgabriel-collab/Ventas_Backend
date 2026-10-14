using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IMovimientoCxCRepository : IRepositorioBase<MovimientoCxC>
    {
        Task<IEnumerable<MovimientoCxC>> ObtenerPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default);
    }
}
