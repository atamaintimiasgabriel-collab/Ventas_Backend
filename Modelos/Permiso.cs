using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    // Capacidades técnicas atómicas del sistema (ej: VENTA_CREAR).
    public class Permiso
    {
        public int IdPermiso { get; set; }
        public string CodigoSistema { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Propiedad de navegación
        public ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
    }
}
