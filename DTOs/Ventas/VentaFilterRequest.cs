namespace SistemaVentasMotorrepuestos.API.DTOs.Ventas
{
    public class VentaFilterRequest
    {
        public int? IdCliente { get; set; }
        public int? IdSucursal { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
