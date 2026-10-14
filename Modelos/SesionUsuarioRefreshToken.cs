using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    /// <summary>
    /// Token de refresh persistido exclusivamente como hash para rotacion y deteccion de reuse.
    /// El token raw nunca debe almacenarse en esta entidad ni en la base de datos.
    /// </summary>
    public class SesionUsuarioRefreshToken
    {
        public long IdRefreshToken { get; set; }
        public int IdSesionUsuario { get; set; }
        public byte[] TokenHash { get; set; } = null!;
        public byte TokenHashVersion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaConsumido { get; set; }
        public long? ReplacedByTokenId { get; set; }
        public DateTime? FechaRevocacion { get; set; }
        public string? MotivoRevocacion { get; set; }
        public DateTime? ReuseDetectedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public SesionUsuario SesionUsuario { get; set; } = null!;
        public SesionUsuarioRefreshToken? ReplacedByToken { get; set; }
        public ICollection<SesionUsuarioRefreshToken> ReplacedTokens { get; set; } = new List<SesionUsuarioRefreshToken>();
    }
}
