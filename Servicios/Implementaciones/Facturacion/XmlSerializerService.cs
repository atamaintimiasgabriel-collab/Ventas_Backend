using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    /// <summary>
    /// Implementación de IXmlSerializerService.
    ///
    /// Flujo interno:
    ///   1. Validar DTO (delegado a UblDocumentMapper).
    ///   2. Mapear DTO → modelo UBL (delegado a UblDocumentMapper).
    ///   3. Serializar modelo UBL → XML con namespaces correctos.
    ///   4. Devolver MemoryStream con Position = 0.
    ///
    /// Garantías:
    ///   - Determinístico: el mismo DTO produce el mismo XML.
    ///   - Sin acceso a BD, sin recálculo de impuestos.
    ///   - No expone stack traces en los errores de negocio.
    ///   - Precision decimal → no hay conversión decimal→double→XML.
    /// </summary>
    public sealed class XmlSerializerService : IXmlSerializerService
    {
        private readonly ILogger<XmlSerializerService> _logger;
        private readonly IXmlSchemaValidator _schemaValidator;
        private readonly ISunatFiscalValidator _fiscalValidator;

        // XmlSerializer puede ser costoso de crear; se cachea por tipo.
        private static readonly Lazy<XmlSerializer> InvoiceSerializer =
            new(() => new XmlSerializer(typeof(UblInvoice)));
        private static readonly Lazy<XmlSerializer> CreditNoteSerializer =
            new(() => new XmlSerializer(typeof(UblCreditNote)));
        private static readonly Lazy<XmlSerializer> DebitNoteSerializer =
            new(() => new XmlSerializer(typeof(UblDebitNote)));

        public XmlSerializerService(
            ILogger<XmlSerializerService> logger,
            IXmlSchemaValidator schemaValidator,
            ISunatFiscalValidator fiscalValidator)
        {
            _logger          = logger ?? throw new ArgumentNullException(nameof(logger));
            _schemaValidator = schemaValidator ?? throw new ArgumentNullException(nameof(schemaValidator));
            _fiscalValidator = fiscalValidator ?? throw new ArgumentNullException(nameof(fiscalValidator));
        }

        /// <inheritdoc/>
        public async Task<Stream> SerializarAXmlAsync(
            DocumentoElectronicoDTO documento,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 1. Mapear DTO → modelo UBL (valida y lanza XmlSerializacionException si hay errores)
            object ublDocument;
            try
            {
                ublDocument = UblDocumentMapper.MapearDesdeDto(documento);
            }
            catch (XmlSerializacionException)
            {
                // Re-lanzar sin wrapping para preservar TipoError
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el mapeo UBL para comprobante {IdComprobante}",
                    documento?.IdComprobante);
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.ErrorDeMapeo,
                    "Ocurrió un error interno durante el mapeo del documento a UBL.",
                    innerException: ex);
            }

            // 2. Serializar modelo UBL → XML Stream
            var memoryStream = new MemoryStream();
            try
            {
                var settings = new XmlWriterSettings
                {
                    Encoding   = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                    Indent     = true,
                    IndentChars = "  ",
                    // No omitir la declaración XML para permitir inspección y validación XSD
                    OmitXmlDeclaration = false,
                    CloseOutput = false, // No cerrar el MemoryStream
                    Async = true
                };

                using var xmlWriter = XmlWriter.Create(memoryStream, settings);
                var serializer = ResolverSerializer(ublDocument);
                serializer.Serialize(xmlWriter, ublDocument);
                await xmlWriter.FlushAsync();
            }
            catch (XmlSerializacionException)
            {
                await memoryStream.DisposeAsync();
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await memoryStream.DisposeAsync();
                _logger.LogError(ex, "Error de serialización XML para comprobante {IdComprobante}",
                    documento?.IdComprobante);
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.ErrorDeSerializacion,
                    "El serializador XML encontró un error al generar el documento.",
                    innerException: ex);
            }
            catch (Exception ex)
            {
                await memoryStream.DisposeAsync();
                _logger.LogError(ex, "Error inesperado al serializar XML para comprobante {IdComprobante}",
                    documento?.IdComprobante);
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.ErrorDeSerializacion,
                    "Error inesperado al serializar el documento XML.",
                    innerException: ex);
            }

            // 3. Validar estructura (well-formed + XSD si disponible)
            memoryStream.Position = 0;
            var validationResult = await _schemaValidator.ValidarAsync(
                memoryStream, documento.TipoComprobante, cancellationToken);

            if (!validationResult.EsValido)
            {
                await memoryStream.DisposeAsync();
                throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.XmlInvalido,
                    "El XML generado no pasó la validación estructural.",
                    validationResult.Errores);
            }

            if (validationResult.XsdNoDisponible)
            {
                _logger.LogInformation(
                    "XSD no disponible para tipo '{TipoComprobante}'. Solo se validó XML well-formed. " +
                    "IdComprobante: {IdComprobante}",
                    documento.TipoComprobante, documento.IdComprobante);
            }

            // 4. Validar conformidad fiscal SUNAT después del XSD y antes de Storage/proveedor.
            memoryStream.Position = 0;
            await _fiscalValidator.ValidarAsync(documento, memoryStream, cancellationToken);

            // 5. Regresar stream al inicio para que el llamador pueda leerlo
            memoryStream.Position = 0;
            return memoryStream;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static XmlSerializer ResolverSerializer(object ublDocument)
        {
            return ublDocument switch
            {
                UblInvoice     => InvoiceSerializer.Value,
                UblCreditNote  => CreditNoteSerializer.Value,
                UblDebitNote   => DebitNoteSerializer.Value,
                _ => throw new XmlSerializacionException(
                    XmlSerializacionErrorTipo.TipoNoSoportado,
                    $"No hay XmlSerializer registrado para el tipo UBL '{ublDocument.GetType().Name}'.")
            };
        }
    }
}
