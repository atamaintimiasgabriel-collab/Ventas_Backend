namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class PedidoDetalle
    {
        public int IdDetallePedido { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal BaseImponible { get; set; }
        public string? ClasificacionTributariaAplicada { get; set; }
        public string? ConceptoTributarioAplicado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalLinea { get; set; }

        // Navegación
        public Pedido Pedido { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
