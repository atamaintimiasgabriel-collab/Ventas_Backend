namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion
{
    public class EmisorDocumentoDTO
    {
        public int IdEmpresa { get; set; }
        public string Ruc { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string? Direccion { get; set; }
    }
}
