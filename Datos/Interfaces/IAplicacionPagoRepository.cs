using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IAplicacionPagoRepository : IRepositorioBase<AplicacionPago>
    {
        Task<IEnumerable<AplicacionPago>> ObtenerPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default);
        Task<IEnumerable<AplicacionPago>> ObtenerPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default);
        Task<decimal> ObtenerMontoAplicadoPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default);
        Task<decimal> ObtenerMontoAplicadoPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken = default);
    }
}
