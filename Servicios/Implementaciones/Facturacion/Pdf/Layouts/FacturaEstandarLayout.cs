using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf.Layouts
{
    public static class FacturaEstandarLayout
    {
        private static readonly XColor Azul = XColor.FromArgb(30, 64, 175);
        private static readonly XColor GrisClaro = XColor.FromArgb(241, 245, 249);
        private static readonly XColor Borde = XColor.FromArgb(203, 213, 225);

        public static void Dibujar(PdfDocument document, DocumentoElectronicoDTO documento, ConfiguracionPdfDTO configuracion)
        {
            var fTitulo = new XFont("Arial", 15, XFontStyleEx.Bold);
            var fSub = new XFont("Arial", 11, XFontStyleEx.Bold);
            var fNormal = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fBold = new XFont("Arial", 9, XFontStyleEx.Bold);
            var fPeq = new XFont("Arial", 8, XFontStyleEx.Regular);

            PdfPage page = NuevaPagina(document, configuracion);
            var gfx = XGraphics.FromPdfPage(page);
            double w = page.Width.Point;
            double h = page.Height.Point;
            const double m = 40;
            double y = m;

            // --- Cabecera: emisor a la izquierda, caja con tipo y numero a la derecha ---
            var emisor = documento.Emisor;
            double anchoCaja = 190;
            double xCaja = w - m - anchoCaja;
            gfx.DrawString(emisor.RazonSocial, fTitulo, XBrushes.Black, new XRect(m, y, xCaja - m - 10, 20), XStringFormats.TopLeft);
            double yEmisor = y + 24;
            if (!string.IsNullOrWhiteSpace(emisor.Direccion))
            {
                gfx.DrawString(PdfFormato.Ajustar(gfx, emisor.Direccion!, fNormal, xCaja - m - 10), fNormal, XBrushes.Black, new XRect(m, yEmisor, xCaja - m - 10, 14), XStringFormats.TopLeft);
                yEmisor += 14;
            }
            if (!string.IsNullOrWhiteSpace(documento.Impresion?.TelefonoEmpresa))
            {
                gfx.DrawString($"Tel: {documento.Impresion!.TelefonoEmpresa}", fNormal, XBrushes.Black, new XRect(m, yEmisor, xCaja - m - 10, 14), XStringFormats.TopLeft);
                yEmisor += 14;
            }
            if (!string.IsNullOrWhiteSpace(documento.Impresion?.Sucursal))
            {
                gfx.DrawString($"Sucursal: {documento.Impresion!.Sucursal}", fNormal, XBrushes.Black, new XRect(m, yEmisor, xCaja - m - 10, 14), XStringFormats.TopLeft);
            }

            double altoCaja = 80;
            gfx.DrawRectangle(new XPen(Azul, 1.5), new XRect(xCaja, y, anchoCaja, altoCaja));
            var centro = new XStringFormat { Alignment = XStringAlignment.Center };
            gfx.DrawString($"R.U.C. {emisor.Ruc}", fSub, XBrushes.Black, new XRect(xCaja, y + 10, anchoCaja, 16), centro);
            gfx.DrawString(PdfFormato.Titulo(documento), fBold, new XSolidBrush(Azul), new XRect(xCaja, y + 32, anchoCaja, 16), centro);
            gfx.DrawString(PdfFormato.Numero(documento), fSub, XBrushes.Black, new XRect(xCaja, y + 52, anchoCaja, 16), centro);

            y += altoCaja + 16;

            // --- Datos del cliente y de la venta ---
            double altoCliente = 62;
            gfx.DrawRectangle(new XPen(Borde, 1), new XSolidBrush(GrisClaro), new XRect(m, y, w - 2 * m, altoCliente));
            double colMitad = m + (w - 2 * m) * 0.62;
            gfx.DrawString("Cliente:", fBold, XBrushes.Black, new XRect(m + 8, y + 6, 60, 12), XStringFormats.TopLeft);
            gfx.DrawString(PdfFormato.Ajustar(gfx, documento.Cliente.RazonSocial, fNormal, colMitad - m - 80), fNormal, XBrushes.Black, new XRect(m + 62, y + 6, colMitad - m - 70, 12), XStringFormats.TopLeft);
            gfx.DrawString($"{documento.Cliente.TipoDocumentoIdentidad}:", fBold, XBrushes.Black, new XRect(m + 8, y + 22, 60, 12), XStringFormats.TopLeft);
            gfx.DrawString(documento.Cliente.NumeroDocumento, fNormal, XBrushes.Black, new XRect(m + 62, y + 22, 200, 12), XStringFormats.TopLeft);
            if (!string.IsNullOrWhiteSpace(documento.Cliente.Direccion))
            {
                gfx.DrawString("Dirección:", fBold, XBrushes.Black, new XRect(m + 8, y + 38, 60, 12), XStringFormats.TopLeft);
                gfx.DrawString(PdfFormato.Ajustar(gfx, documento.Cliente.Direccion!, fNormal, colMitad - m - 80), fNormal, XBrushes.Black, new XRect(m + 62, y + 38, colMitad - m - 70, 12), XStringFormats.TopLeft);
            }

            gfx.DrawString("Emisión:", fBold, XBrushes.Black, new XRect(colMitad, y + 6, 50, 12), XStringFormats.TopLeft);
            gfx.DrawString(PdfFormato.Fecha(documento), fNormal, XBrushes.Black, new XRect(colMitad + 50, y + 6, 120, 12), XStringFormats.TopLeft);
            gfx.DrawString("N° venta:", fBold, XBrushes.Black, new XRect(colMitad, y + 22, 50, 12), XStringFormats.TopLeft);
            gfx.DrawString(documento.Impresion?.NumeroVenta ?? "", fNormal, XBrushes.Black, new XRect(colMitad + 50, y + 22, 120, 12), XStringFormats.TopLeft);
            gfx.DrawString("Cajero:", fBold, XBrushes.Black, new XRect(colMitad, y + 38, 50, 12), XStringFormats.TopLeft);
            gfx.DrawString(documento.Impresion?.Cajero ?? "", fNormal, XBrushes.Black, new XRect(colMitad + 50, y + 38, 120, 12), XStringFormats.TopLeft);
            y += altoCliente + 14;

            // --- Tabla de detalle ---
            double cCod = m, cDesc = m + 72, cCant = w - m - 215, cPu = w - m - 150, cTot = w - m - 75;
            void CabeceraTabla(XGraphics g, double yy)
            {
                g.DrawRectangle(new XSolidBrush(Azul), new XRect(m, yy, w - 2 * m, 18));
                g.DrawString("CÓDIGO", fBold, XBrushes.White, new XRect(cCod + 6, yy + 4, 70, 12), XStringFormats.TopLeft);
                g.DrawString("DESCRIPCIÓN", fBold, XBrushes.White, new XRect(cDesc, yy + 4, 200, 12), XStringFormats.TopLeft);
                g.DrawString("CANT.", fBold, XBrushes.White, new XRect(cCant, yy + 4, 60, 12), XStringFormats.TopRight);
                g.DrawString("P. UNIT.", fBold, XBrushes.White, new XRect(cPu, yy + 4, 70, 12), XStringFormats.TopRight);
                g.DrawString("IMPORTE", fBold, XBrushes.White, new XRect(cTot, yy + 4, 70, 12), XStringFormats.TopRight);
            }

            CabeceraTabla(gfx, y);
            y += 22;
            int fila = 0;
            foreach (var det in documento.Detalles)
            {
                if (y > h - 230)
                {
                    gfx.Dispose();
                    page = NuevaPagina(document, configuracion);
                    gfx = XGraphics.FromPdfPage(page);
                    y = m;
                    CabeceraTabla(gfx, y);
                    y += 22;
                }

                if (fila++ % 2 == 1)
                    gfx.DrawRectangle(new XSolidBrush(GrisClaro), new XRect(m, y - 2, w - 2 * m, 16));

                gfx.DrawString(PdfFormato.Ajustar(gfx, det.CodigoProducto, fNormal, 64), fNormal, XBrushes.Black, new XRect(cCod + 6, y, 66, 12), XStringFormats.TopLeft);
                gfx.DrawString(PdfFormato.Ajustar(gfx, det.Descripcion, fNormal, cCant - cDesc - 10), fNormal, XBrushes.Black, new XRect(cDesc, y, cCant - cDesc, 12), XStringFormats.TopLeft);
                gfx.DrawString(det.Cantidad.ToString("0.##", PdfFormato.Cultura), fNormal, XBrushes.Black, new XRect(cCant, y, 60, 12), XStringFormats.TopRight);
                gfx.DrawString(PdfFormato.Numerico(det.PrecioUnitario), fNormal, XBrushes.Black, new XRect(cPu, y, 70, 12), XStringFormats.TopRight);
                gfx.DrawString(PdfFormato.Numerico(det.Cantidad * det.PrecioUnitario - det.Descuento), fNormal, XBrushes.Black, new XRect(cTot, y, 70, 12), XStringFormats.TopRight);
                y += 16;
            }

            gfx.DrawLine(new XPen(Borde, 1), m, y + 2, w - m, y + 2);
            y += 12;

            // --- Totales (derecha) y forma de pago (izquierda) ---
            double xEtq = w - m - 230;
            double yPago = y;
            gfx.DrawString("FORMA DE PAGO", fBold, new XSolidBrush(Azul), new XRect(m, yPago, 200, 12), XStringFormats.TopLeft);
            yPago += 15;
            foreach (var pago in documento.Impresion?.Pagos ?? new List<PagoImpresionDTO>())
            {
                gfx.DrawString($"{pago.MedioPago}  {PdfFormato.Monto(documento, pago.Monto)}", fNormal, XBrushes.Black, new XRect(m, yPago, xEtq - m - 10, 12), XStringFormats.TopLeft);
                yPago += 12;
                var detalle = PdfFormato.DetallePago(pago);
                if (detalle.Length > 0)
                {
                    gfx.DrawString(PdfFormato.Ajustar(gfx, detalle, fPeq, xEtq - m - 10), fPeq, XBrushes.DimGray, new XRect(m, yPago, xEtq - m - 10, 11), XStringFormats.TopLeft);
                    yPago += 11;
                }
            }
            if (documento.Impresion?.MontoRecibido != null)
            {
                gfx.DrawString($"Recibido {PdfFormato.Monto(documento, documento.Impresion.MontoRecibido.Value)}  ·  Vuelto {PdfFormato.Monto(documento, documento.Impresion.Vuelto ?? 0)}", fNormal, XBrushes.Black, new XRect(m, yPago, xEtq - m - 10, 12), XStringFormats.TopLeft);
            }

            void Linea(string etiqueta, string valor, bool destacada)
            {
                if (destacada)
                {
                    gfx.DrawRectangle(new XSolidBrush(Azul), new XRect(xEtq - 6, y - 3, 236, 20));
                    gfx.DrawString(etiqueta, fBold, XBrushes.White, new XRect(xEtq, y + 1, 110, 14), XStringFormats.TopLeft);
                    gfx.DrawString(valor, fBold, XBrushes.White, new XRect(w - m - 100, y + 1, 94, 14), XStringFormats.TopRight);
                    y += 24;
                }
                else
                {
                    gfx.DrawString(etiqueta, fNormal, XBrushes.Black, new XRect(xEtq, y, 130, 14), XStringFormats.TopLeft);
                    gfx.DrawString(valor, fNormal, XBrushes.Black, new XRect(w - m - 100, y, 94, 14), XStringFormats.TopRight);
                    y += 15;
                }
            }

            Linea("OP. GRAVADAS", PdfFormato.Monto(documento, documento.TotalGravado), false);
            if (documento.TotalDescuento > 0)
                Linea("DESCUENTO", "- " + PdfFormato.Monto(documento, documento.TotalDescuento), false);
            Linea("IGV (18%)", PdfFormato.Monto(documento, documento.TotalIgv), false);
            y += 2;
            Linea("IMPORTE TOTAL", PdfFormato.Monto(documento, documento.ImporteTotal), true);

            // --- Pie ---
            var pie = new XStringFormat { Alignment = XStringAlignment.Center };
            gfx.DrawString(PdfFormato.Pie(documento), fPeq, XBrushes.DimGray, new XRect(m, h - 50, w - 2 * m, 12), pie);
            gfx.DrawString("Gracias por su compra", fPeq, XBrushes.DimGray, new XRect(m, h - 38, w - 2 * m, 12), pie);
            gfx.Dispose();
        }

        private static PdfPage NuevaPagina(PdfDocument document, ConfiguracionPdfDTO configuracion)
        {
            PdfPage page = document.AddPage();

            if (configuracion.Formato == FormatoPdf.A5)
                page.Size = PdfSharp.PageSize.A5;
            else if (configuracion.Formato == FormatoPdf.A3)
                page.Size = PdfSharp.PageSize.A3;
            else if (configuracion.Formato == FormatoPdf.MediaHoja)
            {
                page.Size = PdfSharp.PageSize.A4;
                page.Height = XUnit.FromPoint(page.Height.Point / 2);
            }
            else if (configuracion.Formato == FormatoPdf.Custom)
            {
                page.Width = XUnit.FromMillimeter(configuracion.CustomWidthMm!.Value);
                page.Height = XUnit.FromMillimeter(configuracion.CustomHeightMm!.Value);
            }
            else
                page.Size = PdfSharp.PageSize.A4;

            return page;
        }
    }
}
