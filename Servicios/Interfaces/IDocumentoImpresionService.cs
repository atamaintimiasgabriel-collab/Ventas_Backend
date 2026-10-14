using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IDocumentoImpresionService
    {
        /// <summary>Arma el documento imprimible (boleta, factura o ticket) con los datos reales de la venta.</summary>
        Task<DocumentoElectronicoDTO> ObtenerDocumentoAsync(int idEmpresa, int idVenta, string tipo, decimal? montoRecibido, CancellationToken cancellationToken = default);

        /// <summary>Genera el PDF del documento usando las plantillas existentes (A4 o ticket 80mm).</summary>
        Task<(byte[] Contenido, string NombreArchivo)> GenerarPdfAsync(int idEmpresa, int idVenta, string tipo, decimal? montoRecibido, CancellationToken cancellationToken = default);
    }
}
