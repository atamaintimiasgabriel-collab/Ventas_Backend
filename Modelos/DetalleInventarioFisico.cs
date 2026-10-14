namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class DetalleInventarioFisico
    {
        public int IdDetalleInventario { get; set; }
        public int IdInventarioFisico { get; set; }
        public int IdAlmacen { get; set; }
        public int IdProducto { get; set; }
        public int IdUbicacion { get; set; }
        
        public decimal StockSistema { get; set; }
        public decimal CantidadFisica { get; set; }
        public decimal? Diferencia { get; set; } // Computed column
        public string? Observacion { get; set; }

        public InventarioFisico InventarioFisico { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
        public Ubicacion Ubicacion { get; set; } = null!;
    }
}