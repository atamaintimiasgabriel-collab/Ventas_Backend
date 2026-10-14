namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class MovimientoInventario
    {
        public long IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public int IdAlmacen { get; set; }
        public int IdUbicacion { get; set; }
        public int IdTipoMovimiento { get; set; }
        
        public decimal Cantidad { get; set; }
        public decimal? SaldoAnterior { get; set; }
        public decimal? SaldoActual { get; set; }
        
        public DateTime FechaHora { get; set; }
        public string UsuarioResponsable { get; set; } = null!;
        public string? MotivoObservacion { get; set; }
        public string? IdReferenciaOrigen { get; set; }

        public Producto Producto { get; set; } = null!;
        public Ubicacion Ubicacion { get; set; } = null!;
        public TipoMovimiento TipoMovimiento { get; set; } = null!;
    }
}