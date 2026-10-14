namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion
{
    public enum FormatoPdf
    {
        A3,
        A4,
        A5,
        Ticket,
        MediaHoja,
        Custom
    }

    public class ConfiguracionPdfDTO
    {
        public FormatoPdf Formato { get; set; } = FormatoPdf.A4;
        
        // Dimensiones personalizadas (ej. en milímetros) requeridas cuando Formato == FormatoPdf.Custom
        public double? CustomWidthMm { get; set; }
        public double? CustomHeightMm { get; set; }
        
        // Propiedades adicionales futuras como LogoUrl, ColorPrincipal, etc.
        public string? LogoUrl { get; set; }
        public string? ColorPrincipalHex { get; set; }
    }
}
