namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    /// <summary>
    /// DTO para listar clientes en tabla de listado
    /// </summary>
    public class ClienteListResponse
    {
        public int IdCliente { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string NumeroDocumento { get; set; } = null!;
        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public decimal TotalCompras { get; set; }
        public int CantidadCompras { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
