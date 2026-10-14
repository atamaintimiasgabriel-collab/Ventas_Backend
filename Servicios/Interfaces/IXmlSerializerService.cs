using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    /// <summary>
    /// Contrato para la serialización de un DocumentoElectronicoDTO a un Stream XML.
    /// Responsabilidades:
    ///   - Recibir el DTO ya ensamblado.
    ///   - Mapearlo al modelo UBL interno.
    ///   - Serializar a XML con los namespaces correctos.
    ///   - Devolver el resultado como Stream (no guarda en disco ni en Storage).
    ///
    /// Restricciones explícitas:
    ///   - NO accede a ApplicationDbContext.
    ///   - NO recalcula impuestos.
    ///   - NO llama a ningún proveedor externo (SUNAT, OSE, PSE).
    ///   - NO genera PDF.
    ///   - NO envía emails ni mensajes.
    /// </summary>
    public interface IXmlSerializerService
    {
        /// <summary>
        /// Serializa el DocumentoElectronicoDTO a un Stream XML.
        /// El stream devuelto está posicionado al inicio (Position = 0).
        /// El llamador es responsable de hacer Dispose del stream.
        /// </summary>
        /// <param name="documento">DTO ya construido y validado por DocumentBuilder.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Stream que contiene el XML del documento fiscal.</returns>
        /// <exception cref="XmlSerializacionException">
        ///   Si el DTO es inválido o la serialización falla de forma controlada.
        /// </exception>
        Task<Stream> SerializarAXmlAsync(DocumentoElectronicoDTO documento, CancellationToken cancellationToken = default);
    }
}
