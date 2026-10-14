namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    /// <summary>
    /// DTO para vista detallada de un cliente individual
    /// </summary>
    public class ClienteDetailResponse
    {
        public int IdCliente { get; set; }
        public string TipoPersona { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
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
    }
}
