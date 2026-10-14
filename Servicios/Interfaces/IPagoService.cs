using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IPagoService
    {
        Task<Pago> RegistrarPagoAsync(Pago pago, CancellationToken cancellationToken);
        Task<Pago> ObtenerPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken);
        Task<Pago?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken);
        Task<Pago?> BuscarPorReferenciaExternaAsync(int idEmpresa, int idProveedor, string referenciaExterna, CancellationToken cancellationToken);
        Task ConfirmarPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken);
        Task AnularPagoAsync(int idEmpresa, int idPago, string motivo, CancellationToken cancellationToken);
    }
}
