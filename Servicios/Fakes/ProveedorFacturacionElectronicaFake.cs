using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Fakes
{
    public class ProveedorFacturacionElectronicaFake : IProveedorFacturacionElectronica
    {
        // Propiedades de control para pruebas
        public string SimularEstadoFiscal { get; set; } = "ACEPTADO";
        public bool SimularTimeout { get; set; } = false;
        public bool SimularErrorTemporal { get; set; } = false;

        public async Task<RespuestaEmision> EmitirAsync(DocumentoElectronicoDTO documento, CancellationToken cancellationToken = default)
        {
            if (SimularTimeout)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                // Si el token no cancela, disparamos error de timeout
                throw new TimeoutException("Timeout simulado conectando con el proveedor fiscal.");
            }

            if (SimularErrorTemporal)
            {
                return new RespuestaEmision
                {
                    EsExitoso = false,
                    EstadoFiscal = "NO_ENVIADO",
                    MensajeError = "Error temporal de red o servidor ocupado 503",
                    CodigoError = "503"
                };
            }

            // Simulamos respuesta determinística según la configuración
            return new RespuestaEmision
            {
                EsExitoso = SimularEstadoFiscal == "ACEPTADO",
                EstadoFiscal = SimularEstadoFiscal,
                IdDocumentoExterno = $"DOC-{documento.Serie}-{documento.Correlativo}",
                IdOperacionExterna = Guid.NewGuid().ToString(),
                MensajeError = SimularEstadoFiscal == "RECHAZADO" ? "Documento rechazado por regla fiscal simulada" : null,
                CodigoError = SimularEstadoFiscal == "RECHAZADO" ? "400" : null
            };
        }

        public Task<RespuestaConsulta> ConsultarEstadoAsync(DocumentoElectronicoDTO documento, CancellationToken cancellationToken = default)
        {
            // Simular respuesta de consulta (Ej. si estaba en OBSERVADO o ENVIADO_DESCONOCIDO)
            return Task.FromResult(new RespuestaConsulta
            {
                EsExitoso = true,
                EstadoFiscal = "ACEPTADO",
                MensajeError = null,
                CodigoError = null
            });
        }
    }
}
