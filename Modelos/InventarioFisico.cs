namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class InventarioFisico
    {
        public int IdInventarioFisico { get; set; }
        public int IdAlmacen { get; set; }
        public DateTime FechaProgramada { get; set; }
        public string Estado { get; set; } = null!;
        public string UsuarioResponsable { get; set; } = null!;
        public string? Observacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? UsuarioAprobacion { get; set; }

        public Almacen Almacen { get; set; } = null!;
        public ICollection<DetalleInventarioFisico> Detalles { get; set; } = new List<DetalleInventarioFisico>();
    }
}