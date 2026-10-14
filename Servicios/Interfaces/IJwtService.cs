namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    // Servicio exclusivamente dedicado a la generación de JSON Web Tokens.
    // Desacoplado del Login y de EF Core.
    public interface IJwtService
    {
        string GenerateToken(string idUsuario, string username, int? idEmpresa = null, int? idSucursal = null);
    }
}
