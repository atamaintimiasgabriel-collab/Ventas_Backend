using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    // Clase que representa la identidad de la persona o sistema que accede a la API.
    // Mapea la tabla dbo.Usuarios de SQL Server.
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Activo { get; set; }
        
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; } // Puede ser NULL por el bootstrap inicial
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Propiedad de navegación para la relación N:M con Roles
        public ICollection<UsuarioRol> UsuariosRoles { get; set; } = new List<UsuarioRol>();

        // Propiedad de navegación para la relación 1:N con Sesiones de Usuario
        public ICollection<SesionUsuario> SesionesUsuarios { get; set; } = new List<SesionUsuario>();
    }
}
