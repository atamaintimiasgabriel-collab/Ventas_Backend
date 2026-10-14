using System;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    public static class StorageKeyGenerator
    {
        /// <summary>
        /// Genera una llave de almacenamiento determinística.
        /// </summary>
        /// <param name="empresaId">El ID lógico de la empresa (ej: 'empresa-1').</param>
        /// <param name="fechaEmision">La fecha de emisión del documento para particionar por año/mes.</param>
        /// <param name="tipoDocumento">El tipo de documento (ej: '01' factura, '03' boleta).</param>
        /// <param name="tipoArchivo">El tipo de archivo (ej: 'xml', 'cdr', 'pdf').</param>
        /// <param name="serie">La serie del comprobante (ej: 'F001').</param>
        /// <param name="correlativo">El número correlativo (ej: '00000001').</param>
        /// <param name="extension">La extensión física del archivo (ej: '.xml').</param>
        /// <returns>Una StorageKey lógica.</returns>
        public static string GenerarLlave(
            string empresaId, 
            DateTime fechaEmision, 
            string tipoDocumento, 
            string tipoArchivo, 
            string serie, 
            string correlativo, 
            string extension)
        {
            if (string.IsNullOrWhiteSpace(empresaId)) throw new ArgumentException("EmpresaId no puede ser nulo o vacío.", nameof(empresaId));
            if (string.IsNullOrWhiteSpace(tipoDocumento)) throw new ArgumentException("TipoDocumento no puede ser nulo o vacío.", nameof(tipoDocumento));
            if (string.IsNullOrWhiteSpace(tipoArchivo)) throw new ArgumentException("TipoArchivo no puede ser nulo o vacío.", nameof(tipoArchivo));
            if (string.IsNullOrWhiteSpace(serie)) throw new ArgumentException("Serie no puede ser nulo o vacío.", nameof(serie));
            if (string.IsNullOrWhiteSpace(correlativo)) throw new ArgumentException("Correlativo no puede ser nulo o vacío.", nameof(correlativo));

            // Aseguramos que la extensión comience con punto
            if (!string.IsNullOrWhiteSpace(extension) && !extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string year = fechaEmision.ToString("yyyy");
            string month = fechaEmision.ToString("MM");

            // Formato: {empresaId}/{año}/{mes}/{tipoDocumento}/{tipoArchivo}/{serie}-{correlativo}{extension}
            return $"{empresaId}/{year}/{month}/{tipoDocumento}/{tipoArchivo}/{serie}-{correlativo}{extension}";
        }
    }
}
