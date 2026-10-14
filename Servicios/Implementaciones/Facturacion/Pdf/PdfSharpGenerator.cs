using System.IO;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf.Layouts;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf
{
    public class PdfSharpGenerator : IPdfGenerator
    {
        static PdfSharpGenerator()
        {
            // Requerido para PdfSharp en .NET Core / .NET 5+ para ciertas codificaciones
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (PdfSharp.Fonts.GlobalFontSettings.FontResolver == null)
            {
                PdfSharp.Fonts.GlobalFontSettings.FontResolver = new WindowsFontResolver();
            }
        }

        private class WindowsFontResolver : PdfSharp.Fonts.IFontResolver
        {
            public byte[] GetFont(string faceName)
            {
                string fontPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), faceName + ".ttf");
                if (System.IO.File.Exists(fontPath))
                    return System.IO.File.ReadAllBytes(fontPath);
                return System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), "arial.ttf"));
            }

            public PdfSharp.Fonts.FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                string faceName = familyName.ToLower();
                if (faceName == "arial")
                {
                    if (isBold && isItalic) faceName = "arialbi";
                    else if (isBold) faceName = "arialbd";
                    else if (isItalic) faceName = "ariali";
                    else faceName = "arial";
                }
                return new PdfSharp.Fonts.FontResolverInfo(faceName);
            }
        }

        public Task<byte[]> GenerarPdfAsync(DocumentoElectronicoDTO documento, ConfiguracionPdfDTO configuracion)
        {
            if (configuracion.Formato == FormatoPdf.Custom)
            {
                if (configuracion.CustomWidthMm == null || configuracion.CustomHeightMm == null ||
                    configuracion.CustomWidthMm <= 0 || configuracion.CustomHeightMm <= 0)
                {
                    throw new System.ArgumentException("Dimensiones inválidas para Formato Custom");
                }
            }

            using (var document = new PdfDocument())
            {
                document.Info.Title = $"{PdfFormato.Titulo(documento)} {PdfFormato.Numero(documento)}";
                document.Info.Author = documento.Emisor?.RazonSocial ?? documento.Cliente.RazonSocial;

                // Delegar al Layout según formato
                if (configuracion.Formato == FormatoPdf.Ticket)
                {
                    TicketLayout.Dibujar(document, documento, configuracion);
                }
                else
                {
                    // A3, A4, A5, MediaHoja, Custom se manejan aquí (A4 por defecto en EstandarLayout)
                    FacturaEstandarLayout.Dibujar(document, documento, configuracion);
                }

                // Guardar en un MemoryStream
                using (var ms = new MemoryStream())
                {
                    document.Save(ms, false);
                    return Task.FromResult(ms.ToArray());
                }
            }
        }
    }
}
