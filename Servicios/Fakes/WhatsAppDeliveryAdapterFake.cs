using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Fakes
{
    public class WhatsAppDeliveryAdapterFake : IWhatsAppDeliveryAdapter
    {
        public ComprobanteDeliveryRequestDTO? UltimaSolicitud { get; private set; }
        public bool SimularFallo { get; set; }

        public Task<ComprobanteDeliveryResultDTO> EnviarAsync(
            ComprobanteDeliveryRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UltimaSolicitud = request;

            return Task.FromResult(new ComprobanteDeliveryResultDTO
            {
                Exito = !SimularFallo,
                Canal = "WHATSAPP",
                Codigo = SimularFallo ? "WHATSAPP_FAKE_ERROR" : "DELIVERED",
                Mensaje = SimularFallo
                    ? "El adapter WhatsApp Fake simuló un fallo controlado."
                    : "Entrega WhatsApp simulada correctamente.",
                IdentificadorExterno = SimularFallo
                    ? null
                    : $"WHATSAPP-FAKE-{request.IdComprobante:N}"
            });
        }
    }
}
