using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SistemaVentasMotorrepuestos.API.Servicios.Autorizacion
{
    // Proveedor dinámico de políticas de autorización.
    // Evita tener que registrar manualmente AddPolicy() por cada posible permiso.
    public class PermisoPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermisoPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Primero, verificar si la policy ya fue configurada explícitamente en Startup
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null)
            {
                return policy;
            }

            // Si no existe, se asume que el nombre de la policy es un CodigoSistema de permiso.
            // Ejemplo: [Authorize(Policy = "VENTA_CREAR")]
            // Se construye dinámicamente la policy inyectando el PermisoRequirement correspondiente.
            var builder = new AuthorizationPolicyBuilder();
            builder.AddRequirements(new PermisoRequirement(policyName));
            return builder.Build();
        }
    }
}
