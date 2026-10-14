using Microsoft.AspNetCore.Identity;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    // Implementación estándar usando el PasswordHasher de ASP.NET Core Identity.
    // Se eligió esta implementación oficial de Microsoft porque:
    // 1. Está integrada en el framework (cero dependencias externas).
    // 2. Es criptográficamente robusta (maneja Salt automáticamente).
    // 3. Genera un hash seguro que cabe perfectamente en un VARCHAR(255) (usualmente mide ~84 caracteres).
    // NOTA: Esta clase transforma contraseñas de ida y valida (hashing, no cifrado reversible).
    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<Usuario> _passwordHasher;
        
        // El Hasher de Identity requiere genéricamente una instancia de TUser. 
        // Usamos una instancia vacía (dummy) para la compatibilidad con el diseño del framework.
        private readonly Usuario _dummyUser;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<Usuario>();
            _dummyUser = new Usuario(); 
        }

        public string HashPassword(string password)
        {
            // Genera el hash de la contraseña. El algoritmo incluye un 'salt' aleatorio
            // de forma transparente, por lo que la misma contraseña generará hashes distintos.
            return _passwordHasher.HashPassword(_dummyUser, password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try 
            {
                // El Hasher internamente extrae el 'salt' del string hash y realiza la computación.
                var result = _passwordHasher.VerifyHashedPassword(_dummyUser, hash, password);
                return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch
            {
                // Si el formato del hash almacenado es inválido, evitamos filtrar información en excepciones.
                return false;
            }
        }
    }
}
