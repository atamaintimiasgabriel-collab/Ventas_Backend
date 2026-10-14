using SistemaVentasMotorrepuestos.API.DTOs.Autenticacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    // Abstracción del flujo principal de autenticación.
    // Desacopla la lógica de negocio del AuthController.
    public interface IAuthService
    {
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
        Task<CurrentAuthContextDto?> GetCurrentContextAsync();
    }
}
