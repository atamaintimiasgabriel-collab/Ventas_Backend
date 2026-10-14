using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    // Opciones fuertemente tipadas para JWT.
    // Utiliza DataAnnotations para garantizar que la API no arranque con configuración insegura o faltante.
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required(ErrorMessage = "La clave secreta (Key) es obligatoria.")]
        [MinLength(32, ErrorMessage = "La clave secreta debe tener al menos 32 caracteres (256 bits) para garantizar un algoritmo HMAC SHA-256 seguro.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Emisor (Issuer) es obligatorio.")]
        public string Issuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Audiencia (Audience) es obligatoria.")]
        public string Audience { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Los minutos de expiración deben ser mayores a 0.")]
        public int ExpirationMinutes { get; set; }
    }
}
