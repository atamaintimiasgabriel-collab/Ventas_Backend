using Microsoft.AspNetCore.Authorization;

namespace SistemaVentasMotorrepuestos.API.Servicios.Autorizacion
{
    // Representa el requerimiento de poseer un permiso específico (basado en el CodigoSistema).
    public class PermisoRequirement : IAuthorizationRequirement
    {
        public string CodigoSistema { get; }

        public PermisoRequirement(string codigoSistema)
        {
            CodigoSistema = codigoSistema;
        }
    }
}
