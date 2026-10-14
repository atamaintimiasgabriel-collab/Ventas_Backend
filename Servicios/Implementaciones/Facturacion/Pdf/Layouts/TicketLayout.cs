using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf.Layouts
{
    public static class TicketLayout
    {
        public static void Dibujar(PdfDocument document, DocumentoElectronicoDTO documento, ConfiguracionPdfDTO configuracion)
        {
            PdfPage page = document.AddPage();

            // 80 mm de ancho (226 pt); el alto se calcula segun el contenido.
            const double ancho = 226;
            const double margen = 10;
            double contenido = ancho - 2 * margen;
            var pagos = documento.Impresion?.Pagos ?? new List<PagoImpresionDTO>();
            double alto = 300 + documento.Detalles.Count * 24 + pagos.Count * 24;
            page.Width = XUnit.FromPoint(ancho);
            page.Height = XUnit.FromPoint(Math.Max(alto, 380));

            using var gfx = XGraphics.FromPdfPage(page);
            var fTitulo = new XFont("Arial", 11, XFontStyleEx.Bold);
            var fNormal = new XFont("Arial", 8, XFontStyleEx.Regular);
            var fBold = new XFont("Arial", 8, XFontStyleEx.Bold);
            var fTotal = new XFont("Arial", 10, XFontStyleEx.Bold);
            var centro = new XStringFormat { Alignment = XStringAlignment.Center };
            var pluma = new XPen(XColors.Black, 0.5) { DashStyle = XDashStyle.Dash };

            double y = 14;
            void Centrado(string texto, XFont fuente, double alt)
            {
                gfx.DrawString(PdfFormato.Ajustar(gfx, texto, fuente, contenido), fuente, XBrushes.Black, new XRect(margen, y, contenido, alt), centro);
                y += alt + 2;
            }
            void Par(string izq, string der, XFont fuente)
            {
                gfx.DrawString(izq, fuente, XBrushes.Black, new XRect(margen, y, contenido, 11), XStringFormats.TopLeft);
                gfx.DrawString(der, fuente, XBrushes.Black, new XRect(margen, y, contenido, 11), XStringFormats.TopRight);
                y += 12;
            }
            void Separador()
            {
                gfx.DrawLine(pluma, margen, y, ancho - margen, y);
                y += 6;
            }

            // --- Cabecera ---
            Centrado(documento.Emisor.RazonSocial, fTitulo, 14);
            Centrado($"RUC: {documento.Emisor.Ruc}", fNormal, 11);
            if (!string.IsNullOrWhiteSpace(documento.Emisor.Direccion)) Centrado(documento.Emisor.Direccion!, fNormal, 11);
            if (!string.IsNullOrWhiteSpace(documento.Impresion?.TelefonoEmpresa)) Centrado($"Tel: {documento.Impresion!.TelefonoEmpresa}", fNormal, 11);
            y += 4;
            Separador();
            Centrado(PdfFormato.Titulo(documento), fBold, 12);
            Centrado(PdfFormato.Numero(documento), fBold, 12);
            Separador();

            // --- Datos de la venta ---
            Par("Fecha:", PdfFormato.Fecha(documento), fNormal);
            if (!PdfFormato.EsTicket(documento)) Par("N° venta:", documento.Impresion?.NumeroVenta ?? "", fNormal);
            Par("Cajero:", documento.Impresion?.Cajero ?? "", fNormal);
            gfx.DrawString(PdfFormato.Ajustar(gfx, $"Cliente: {documento.Cliente.RazonSocial}", fNormal, contenido), fNormal, XBrushes.Black, new XRect(margen, y, contenido, 11), XStringFormats.TopLeft);
            y += 12;
            Par($"{documento.Cliente.TipoDocumentoIdentidad}:", documento.Cliente.NumeroDocumento, fNormal);
            y += 2;
            Separador();

            // --- Detalle ---
            Par("CANT  DESCRIPCIÓN", "IMPORTE", fBold);
            foreach (var det in documento.Detalles)
            {
                gfx.DrawString(PdfFormato.Ajustar(gfx, det.Descripcion, fNormal, contenido), fNormal, XBrushes.Black, new XRect(margen, y, contenido, 11), XStringFormats.TopLeft);
                y += 11;
                Par($"{det.Cantidad.ToString("0.##", PdfFormato.Cultura)} x {PdfFormato.Numerico(det.PrecioUnitario)}", PdfFormato.Numerico(det.Cantidad * det.PrecioUnitario - det.Descuento), fNormal);
            }
            y += 2;
            Separador();

            // --- Totales ---
            Par("OP. GRAVADAS", PdfFormato.Monto(documento, documento.TotalGravado), fNormal);
            if (documento.TotalDescuento > 0) Par("DESCUENTO", "- " + PdfFormato.Monto(documento, documento.TotalDescuento), fNormal);
            Par("IGV (18%)", PdfFormato.Monto(documento, documento.TotalIgv), fNormal);
            y += 2;
            Par("TOTAL", PdfFormato.Monto(documento, documento.ImporteTotal), fTotal);
            y += 4;
            Separador();

            // --- Pago ---
            foreach (var pago in pagos)
            {
                Par(pago.MedioPago, PdfFormato.Monto(documento, pago.Monto), fNormal);
                var detalle = PdfFormato.DetallePago(pago);
                if (detalle.Length > 0)
                {
                    gfx.DrawString(PdfFormato.Ajustar(gfx, detalle, fNormal, contenido), fNormal, XBrushes.DimGray, new XRect(margen, y, contenido, 11), XStringFormats.TopLeft);
                    y += 11;
                }
            }
            if (documento.Impresion?.MontoRecibido != null)
            {
                Par("Recibido", PdfFormato.Monto(documento, documento.Impresion.MontoRecibido.Value), fNormal);
                Par("Vuelto", PdfFormato.Monto(documento, documento.Impresion.Vuelto ?? 0), fBold);
            }

            y += 8;
            Centrado(PdfFormato.Pie(documento), fNormal, 11);
            Centrado("¡Gracias por su compra!", fBold, 12);
        }
    }
}
