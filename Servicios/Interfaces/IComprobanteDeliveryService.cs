using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IComprobanteDeliveryService
    {
        Task<ComprobanteDeliveryResultDTO> EnviarAsync(
            Comprobante comprobante,
            ComprobanteDeliveryRequestDTO request,
            CancellationToken cancellationToken = default);
    }
}
