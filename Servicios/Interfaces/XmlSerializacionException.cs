using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    /// <summary>
    /// Excepción controlada que distingue los distintos tipos de error
    /// del proceso de serialización XML.
    /// No expone stack traces en el mensaje público.
    /// </summary>
    public class XmlSerializacionException : Exception
    {
        public XmlSerializacionErrorTipo TipoError { get; }
        public IReadOnlyList<string> Detalles { get; }

        public XmlSerializacionException(
            XmlSerializacionErrorTipo tipoError,
            string mensaje,
            IEnumerable<string>? detalles = null,
            Exception? innerException = null)
            : base(mensaje, innerException)
        {
            TipoError = tipoError;
            Detalles = (detalles ?? Array.Empty<string>()).ToList().AsReadOnly();
        }
    }

    public enum XmlSerializacionErrorTipo
    {
        /// <summary>El DTO tiene campos nulos o vacíos obligatorios.</summary>
        DtoInvalido,

        /// <summary>Un dato obligatorio no está presente (ej. Serie vacía).</summary>
        DatoObligatiorioFaltante,

        /// <summary>El tipo de comprobante no está soportado por el mapper.</summary>
        TipoNoSoportado,

        /// <summary>Falló el mapeo de DTO a modelo UBL.</summary>
        ErrorDeMapeo,

        /// <summary>El XmlSerializer lanzó una excepción al serializar.</summary>
        ErrorDeSerializacion,

        /// <summary>El XML generado no pasó la validación estructural.</summary>
        XmlInvalido,

        /// <summary>El XSD no está disponible para validar este tipo de documento.</summary>
        XsdNoDisponible,
    }
}
