namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class VentaDetalle
    {
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalLinea { get; set; }

        public Venta Venta { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
