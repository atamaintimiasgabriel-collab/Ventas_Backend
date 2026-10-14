using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IProveedorFacturacionElectronica
    {
        Task<RespuestaEmision> EmitirAsync(DocumentoElectronicoDTO documento, CancellationToken cancellationToken = default);
        Task<RespuestaConsulta> ConsultarEstadoAsync(DocumentoElectronicoDTO documento, CancellationToken cancellationToken = default);
    }
}
