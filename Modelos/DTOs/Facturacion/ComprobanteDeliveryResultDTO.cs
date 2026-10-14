namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion
{
    public class ComprobanteDeliveryResultDTO
    {
        public bool Exito { get; set; }
        public string Canal { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string? IdentificadorExterno { get; set; }
    }
}
