namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    // Abstrae el origen de la identidad del usuario actual (ej. extraída del HttpContext vía JWT).
    // Permite que capas inferiores (como Datos) puedan acceder a quién está ejecutando
    // una acción sin acoplarse directamente a mecanismos de autenticación web o Claims.
    public interface ICurrentUserService
    {
        string? IdUsuario { get; }
        string? Username { get; }
        int? IdEmpresa { get; }
        bool IsAuthenticated { get; }
    }
}
