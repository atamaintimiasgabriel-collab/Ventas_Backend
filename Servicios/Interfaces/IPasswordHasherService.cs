namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    // Abstracción profesional para el manejo seguro de contraseñas.
    // Mantiene la arquitectura limpia y la responsabilidad separada: no depende de JWT, Login ni DbContext.
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
