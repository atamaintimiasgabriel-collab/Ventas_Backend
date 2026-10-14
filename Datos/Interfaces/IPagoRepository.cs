using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IPagoRepository : IRepositorioBase<Pago>
    {
        Task<Pago?> ObtenerPorIdAsync(int idEmpresa, int idPago, CancellationToken cancellationToken = default);
        Task<Pago?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken = default);
        Task<Pago?> BuscarPorReferenciaExternaAsync(int idEmpresa, int idProveedor, string referenciaExterna, CancellationToken cancellationToken = default);
        Task<IEnumerable<Pago>> ObtenerPorClienteAsync(int idEmpresa, int idCliente, CancellationToken cancellationToken = default);
    }
}
