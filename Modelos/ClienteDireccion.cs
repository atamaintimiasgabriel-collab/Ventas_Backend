namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ClienteDireccion
    {
        public int IdDireccion { get; set; }
        public int IdCliente { get; set; }
        public string TipoDireccion { get; set; } = null!;    // PRINCIPAL, FACTURACION, ENVIO, OTRA
        public string DireccionCompleta { get; set; } = null!;
        public string? Referencia { get; set; }
        public string? Distrito { get; set; }
        public string? Provincia { get; set; }
        public string? Departamento { get; set; }
        public string? CodigoPostal { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }

        // Navegación
        public Cliente Cliente { get; set; } = null!;
    }
}
