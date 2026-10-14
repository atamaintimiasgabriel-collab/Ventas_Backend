namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Ubicacion
    {
        public int IdUbicacion { get; set; }
        public int IdAlmacen { get; set; }
        public string Codigo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }

        public Almacen Almacen { get; set; } = null!;
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
        public ICollection<DetalleTransferencia> DetallesTransferenciaOrigen { get; set; } = new List<DetalleTransferencia>();
        public ICollection<DetalleTransferencia> DetallesTransferenciaDestino { get; set; } = new List<DetalleTransferencia>();
        public ICollection<DetalleInventarioFisico> DetallesInventarioFisico { get; set; } = new List<DetalleInventarioFisico>();
    }
}