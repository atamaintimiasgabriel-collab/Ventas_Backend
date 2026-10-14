using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IObligacionCxCRepository : IRepositorioBase<ObligacionCxC>
    {
        Task<ObligacionCxC?> ObtenerPorIdAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default);
        Task<ObligacionCxC?> ObtenerPorVentaAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default);
        Task<IEnumerable<ObligacionCxC>> ObtenerPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default);
        Task<IEnumerable<ObligacionCxC>> ObtenerPendientesPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default);
    }
}
