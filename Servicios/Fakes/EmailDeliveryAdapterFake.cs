using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Fakes
{
    public class EmailDeliveryAdapterFake : IEmailDeliveryAdapter
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
                Canal = "EMAIL",
                Codigo = SimularFallo ? "EMAIL_FAKE_ERROR" : "DELIVERED",
                Mensaje = SimularFallo
                    ? "El adapter Email Fake simuló un fallo controlado."
                    : "Entrega Email simulada correctamente.",
                IdentificadorExterno = SimularFallo
                    ? null
                    : $"EMAIL-FAKE-{request.IdComprobante:N}"
            });
        }
    }
}
