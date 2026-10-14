using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Autorizacion
{
    // Manejador que evalúa si un usuario autenticado posee el permiso requerido (CodigoSistema)
    // consultando la infraestructura relacional de roles y permisos.
    public class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
    {
        private readonly IPermisoAuthorizationService _permisoService;

        // IAuthorizationHandler será registrado como Scoped en DI.
        public PermisoAuthorizationHandler(IPermisoAuthorizationService permisoService)
        {
            _permisoService = permisoService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
        {
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return; // Rechazo inmediato si no está autenticado
            }

            // Extraemos el ID del usuario directamente del token (establecido en JwtService)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return; 
            }

            // Comprobación lógica delegada al servicio especializado.
            // Evitamos que el Handler construya consultas EF, aislando la capa de datos.
            bool tienePermiso = await _permisoService.UsuarioTienePermisoAsync(usuarioId, requirement.CodigoSistema);

            if (tienePermiso)
            {
                // El usuario posee el permiso solicitado mediante al menos uno de sus roles activos
                context.Succeed(requirement);
            }
        }
    }
}
