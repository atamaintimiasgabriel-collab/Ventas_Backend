namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Almacen
    {
        public int IdAlmacen { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public int? IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        public ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();
        public ICollection<Transferencia> TransferenciasOrigen { get; set; } = new List<Transferencia>();
        public ICollection<Transferencia> TransferenciasDestino { get; set; } = new List<Transferencia>();
        public ICollection<InventarioFisico> InventariosFisicos { get; set; } = new List<InventarioFisico>();
    }
}