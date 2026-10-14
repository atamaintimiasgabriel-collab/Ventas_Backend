using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion
{
    public class ComprobanteDeliveryRequestDTO
    {
        public int IdEmpresa { get; set; }
        public Guid IdComprobante { get; set; }
        public string Canal { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? Asunto { get; set; }
        public string? Mensaje { get; set; }
        public IReadOnlyList<DeliveryArtifactReferenceDTO> Artefactos { get; set; }
            = Array.Empty<DeliveryArtifactReferenceDTO>();
    }
}
