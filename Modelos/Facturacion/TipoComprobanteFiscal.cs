using System.Globalization;
using System.Text;

namespace SistemaVentasMotorrepuestos.API.Modelos.Facturacion
{
    public static class TipoComprobanteFiscal
    {
        public const string Factura = "01";
        public const string Boleta = "03";
        public const string NotaCredito = "07";
        public const string NotaDebito = "08";

        public static string Normalizar(string tipoComprobante)
        {
            if (string.IsNullOrWhiteSpace(tipoComprobante))
                throw new ArgumentException("El tipo de comprobante fiscal es obligatorio.", nameof(tipoComprobante));

            var alias = RemoverDiacriticos(tipoComprobante.Trim()).ToUpperInvariant();

            return alias switch
            {
                Factura or "FACTURA" => Factura,
                Boleta or "BOLETA" => Boleta,
                NotaCredito or "NC" or "NOTA DE CREDITO" => NotaCredito,
                NotaDebito or "ND" or "NOTA DE DEBITO" => NotaDebito,
                _ => throw new ArgumentException(
                    $"El tipo de comprobante fiscal '{tipoComprobante}' no está soportado.",
                    nameof(tipoComprobante))
            };
        }

        private static string RemoverDiacriticos(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
