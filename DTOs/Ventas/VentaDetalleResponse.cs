namespace SistemaVentasMotorrepuestos.API.DTOs.Ventas
{
    public class VentaDetalleResponse
    {
        public int IdDetalleVenta { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
