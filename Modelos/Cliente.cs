namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public int IdEmpresa { get; set; }
        public string TipoPersona { get; set; } = null!;       // 'N' = Natural, 'J' = Jurídica
        public string TipoDocumento { get; set; } = null!;      // DNI, RUC, CE, PAS, OTR
        public string NumeroDocumento { get; set; } = null!;
        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public decimal? LimiteCredito { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Navegación
        public Empresa Empresa { get; set; } = null!;
        public ICollection<ClienteContacto> Contactos { get; set; } = new List<ClienteContacto>();
        public ICollection<ClienteDireccion> Direcciones { get; set; } = new List<ClienteDireccion>();
    }
}
