using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    /// <summary>
    /// Sesion durable de usuario para agrupar access tokens futuros y familia de refresh tokens.
    /// No fija empresa ni sucursal: el contexto tenant vive en el access token y se revalida server-side.
    /// </summary>
    public class SesionUsuario
    {
        public int IdSesionUsuario { get; set; }
        public Guid SessionId { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaUltimaActividad { get; set; }
        public DateTime? FechaRevocacion { get; set; }
        public string? MotivoRevocacion { get; set; }
        public string? RevocadaPor { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public Usuario Usuario { get; set; } = null!;
        public ICollection<SesionUsuarioRefreshToken> RefreshTokens { get; set; } = new List<SesionUsuarioRefreshToken>();
    }
}
