namespace SistemaVentasMotorrepuestos.API.Modelos
{
    // Entidad intermedia para la relación N:M entre Rol y Permiso.
    public class RolPermiso
    {
        public int IdRol { get; set; }
        public int IdPermiso { get; set; }

        // Propiedades de navegación
        public Rol Rol { get; set; } = null!;
        public Permiso Permiso { get; set; } = null!;
    }
}
