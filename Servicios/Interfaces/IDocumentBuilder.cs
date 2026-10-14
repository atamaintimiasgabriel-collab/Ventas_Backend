using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IDocumentBuilder
    {
        // Compatibilidad del ConsultorWorker 5.5: este DTO se usa sólo para consultar
        // al proveedor Fake y no es apto para serialización XML.
        Task<DocumentoElectronicoDTO> ConstruirDesdeVentaAsync(
            Venta venta,
            Comprobante comprobante,
            CancellationToken cancellationToken = default);

        Task<DocumentoElectronicoDTO> ConstruirDesdeVentaAsync(
            Venta venta,
            Comprobante comprobante,
            Empresa empresa,
            CancellationToken cancellationToken = default);
    }
}
