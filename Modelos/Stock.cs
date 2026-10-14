namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Stock
    {
        public int IdStock { get; set; }
        public int IdProducto { get; set; }
        public int IdAlmacen { get; set; }
        public int IdUbicacion { get; set; }
        
        public decimal StockFisico { get; set; }
        public decimal StockReservado { get; set; }
        public decimal? StockDisponible { get; set; } // Computed column
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }
        public decimal StockSeguridad { get; set; }
        
        public string? Lote { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public Producto Producto { get; set; } = null!;
        public Ubicacion Ubicacion { get; set; } = null!;
    }
}