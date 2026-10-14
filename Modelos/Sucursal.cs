namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Sucursal
    {
        public int IdSucursal { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; } = null!;
        public string? CodigoEstablecimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public ICollection<Almacen> Almacenes { get; set; } = new List<Almacen>();
    }
}
