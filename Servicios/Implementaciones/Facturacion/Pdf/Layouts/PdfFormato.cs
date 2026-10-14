using System.Globalization;
using PdfSharp.Drawing;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf.Layouts
{
    /// <summary>Textos y utilidades compartidas por las plantillas A4 y ticket.</summary>
    internal static class PdfFormato
    {
        public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-PE");

        public static string Titulo(DocumentoElectronicoDTO d) => d.TipoComprobante switch
        {
            "01" or "FACTURA" => "FACTURA ELECTRÓNICA",
            "03" or "BOLETA" => "BOLETA DE VENTA ELECTRÓNICA",
            "TICKET" => "TICKET DE VENTA",
            _ => d.TipoComprobante.ToUpperInvariant()
        };

        public static bool EsTicket(DocumentoElectronicoDTO d) => d.TipoComprobante == "TICKET";

        public static string Numero(DocumentoElectronicoDTO d)
        {
            if (EsTicket(d)) return d.Impresion?.NumeroVenta ?? string.Empty;
            return d.Correlativo > 0 ? $"{d.Serie}-{d.Correlativo:D8}" : $"{d.Serie}-(en proceso)";
        }

        public static string Monto(DocumentoElectronicoDTO d, decimal valor)
            => $"{(d.Moneda == "PEN" ? "S/" : d.Moneda)} {valor.ToString("N2", Cultura)}";

        public static string Numerico(decimal valor) => valor.ToString("N2", Cultura);

        public static string Fecha(DocumentoElectronicoDTO d) => d.FechaEmision.ToString("dd/MM/yyyy HH:mm", Cultura);

        public static string Pie(DocumentoElectronicoDTO d) => EsTicket(d)
            ? "Ticket interno sin valor tributario."
            : $"Representación impresa de la {Titulo(d).ToLowerInvariant()}.";

        public static string DetallePago(PagoImpresionDTO p)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(p.Adquirente)) partes.Add(p.Adquirente!);
            if (!string.IsNullOrWhiteSpace(p.Ultimos4Digitos)) partes.Add("****" + p.Ultimos4Digitos);
            if (!string.IsNullOrWhiteSpace(p.CodigoAutorizacion)) partes.Add("Aut. " + p.CodigoAutorizacion);
            if (!string.IsNullOrWhiteSpace(p.Referencia)) partes.Add("Op. " + p.Referencia);
            return string.Join(" · ", partes);
        }

        /// <summary>Recorta el texto con puntos suspensivos para que quepa en el ancho indicado.</summary>
        public static string Ajustar(XGraphics gfx, string texto, XFont fuente, double ancho)
        {
            if (string.IsNullOrEmpty(texto) || gfx.MeasureString(texto, fuente).Width <= ancho) return texto;
            var recortado = texto;
            while (recortado.Length > 1 && gfx.MeasureString(recortado + "…", fuente).Width > ancho)
                recortado = recortado[..^1];
            return recortado.TrimEnd() + "…";
        }
    }
}
