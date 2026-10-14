using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    /// <summary>
    /// Contrato para la validación estructural de un XML contra esquema XSD.
    ///
    /// La ruta productiva usa schemas UBL 2.1 locales y falla de forma
    /// controlada si el paquete requerido no está disponible.
    /// </summary>
    public interface IXmlSchemaValidator
    {
        /// <summary>
        /// Valida el XML contenido en el stream contra el esquema correspondiente al tipo de comprobante.
        /// El stream NO se consume (se restaura a Position = 0 tras la lectura).
        /// </summary>
        Task<XmlValidationResult> ValidarAsync(Stream xmlStream, string tipoComprobante, CancellationToken cancellationToken = default);
    }

    public class XmlValidationResult
    {
        public bool EsValido { get; init; }

        /// <summary>Compatibilidad con validadores auxiliares no productivos sin XSD.</summary>
        public bool XsdNoDisponible { get; init; }

        /// <summary>Lista de errores de validación. Vacía si EsValido = true.</summary>
        public IReadOnlyList<string> Errores { get; init; } = Array.Empty<string>();

        public static XmlValidationResult Valido() =>
            new() { EsValido = true };

        public static XmlValidationResult SinXsd() =>
            new() { EsValido = true, XsdNoDisponible = true };

        public static XmlValidationResult Invalido(IEnumerable<string> errores) =>
            new() { EsValido = false, Errores = errores.ToList().AsReadOnly() };
    }
}
