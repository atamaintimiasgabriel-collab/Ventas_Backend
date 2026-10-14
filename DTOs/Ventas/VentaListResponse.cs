namespace SistemaVentasMotorrepuestos.API.DTOs.Ventas
{
    public class VentaListResponse
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public decimal Total { get; set; }
        public string Moneda { get; set; } = null!;
        public DateTime FechaVenta { get; set; }
    }
}
