using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion
{
    public interface IEmailDeliveryAdapter
    {
        Task<ComprobanteDeliveryResultDTO> EnviarAsync(
            ComprobanteDeliveryRequestDTO request,
            CancellationToken cancellationToken = default);
    }
}
