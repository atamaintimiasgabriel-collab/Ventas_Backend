namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ClienteContacto
    {
        public int IdContacto { get; set; }
        public int IdCliente { get; set; }
        public string TipoContacto { get; set; } = null!;    // TELEFONO, EMAIL, WHATSAPP, OTRO
        public string Valor { get; set; } = null!;
        public bool EsPrincipal { get; set; }
        public string? Observacion { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }

        // Navegación
        public Cliente Cliente { get; set; } = null!;
    }
}
