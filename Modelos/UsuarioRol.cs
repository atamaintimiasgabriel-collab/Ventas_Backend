namespace SistemaVentasMotorrepuestos.API.Modelos
{
    // Entidad intermedia para la relación N:M entre Usuario y Rol.
    // Evita el diseño rígido al permitir que un usuario posea múltiples roles.
    public class UsuarioRol
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }

        // Propiedades de navegación
        public Usuario Usuario { get; set; } = null!;
        public Rol Rol { get; set; } = null!;
    }
}
