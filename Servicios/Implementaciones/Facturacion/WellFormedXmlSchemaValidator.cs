using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    /// <summary>
    /// Validador de esquema XML de solo-estructura (well-formed XML).
    ///
    /// Los XSD oficiales de SUNAT/OSE requieren licencia o descarga de portales
    /// gubernamentales. Esta implementación valida que el XML sea well-formed
    /// y retorna XsdNoDisponible = true para indicar que la validación XSD
    /// completa está pendiente de ser integrada.
    ///
    /// Esta clase NUNCA llama a servicios externos.
    /// </summary>
    public sealed class WellFormedXmlSchemaValidator : IXmlSchemaValidator
    {
        private static readonly HashSet<string> TiposKnown = new(StringComparer.OrdinalIgnoreCase)
        {
            "01", "03", "07", "08"
        };

        /// <inheritdoc/>
        public async Task<XmlValidationResult> ValidarAsync(
            Stream xmlStream,
            string tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (xmlStream is null || xmlStream.Length == 0)
                return XmlValidationResult.Invalido(new[] { "El stream XML está vacío." });

            // Preservar la posición original
            var originalPosition = xmlStream.CanSeek ? xmlStream.Position : 0;

            try
            {
                // Validar solo well-formed XML
                var settings = new XmlReaderSettings
                {
                    Async = true,
                    DtdProcessing = DtdProcessing.Ignore,
                    // Sin esquemas: solo valida well-formed
                    ValidationType = System.Xml.ValidationType.None,
                    IgnoreWhitespace = false,
                };

                using var reader = XmlReader.Create(xmlStream, settings);

                var errores = new List<string>();
                try
                {
                    while (await reader.ReadAsync())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (XmlException ex)
                {
                    errores.Add($"XML malformado en línea {ex.LineNumber}, columna {ex.LinePosition}: {ex.Message}");
                }

                if (errores.Count > 0)
                    return XmlValidationResult.Invalido(errores);
            }
            finally
            {
                // Restaurar posición para que el llamador pueda releer el stream
                if (xmlStream.CanSeek)
                    xmlStream.Position = originalPosition;
            }

            // XML well-formed. Los XSD oficiales de SUNAT no están disponibles.
            return XmlValidationResult.SinXsd();
        }
    }
}
