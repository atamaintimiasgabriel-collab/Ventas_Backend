namespace SistemaVentasMotorrepuestos.API.DTOs.Ventas
{
    public class VentaResponse
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; } = null!;
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public int IdCliente { get; set; }
        public int? IdPedido { get; set; }
        public string Origen { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Moneda { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public string? Observaciones { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public List<VentaDetalleResponse> Detalles { get; set; } = new();
    }
}
