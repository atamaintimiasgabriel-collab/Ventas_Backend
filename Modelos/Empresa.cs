namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Empresa
    {
        public int IdEmpresa { get; set; }
        public string RazonSocial { get; set; } = null!;
        public string RucDocumento { get; set; } = null!;
        public string? DireccionFiscal { get; set; }
        public string? Telefono { get; set; }
        public string? EmailContacto { get; set; }
        public string? NombreComercial { get; set; }
        public string? SitioWeb { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string? ConfiguracionFiscalJson { get; set; }

        public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
    }
}
