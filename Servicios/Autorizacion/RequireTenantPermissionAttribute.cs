using Microsoft.AspNetCore.Authorization;

namespace SistemaVentasMotorrepuestos.API.Servicios.Autorizacion
{
    /// <summary>
    /// Atributo de autorización que requiere una permiso específico para tenant actual.
    /// El proveedor de políticas (PermisoPolicyProvider) interpreta el código del permiso
    /// como nombre de política y valida mediante PermisoRequirement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireTenantPermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Inicializa una nueva instancia del atributo con el código de permiso requerido.
        /// </summary>
        /// <param name="permissionCode">Código del permiso (ej: "CLIENTES_CONSULTAR")</param>
        public RequireTenantPermissionAttribute(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                throw new ArgumentException("El código de permiso no puede ser nulo o vacío.", nameof(permissionCode));
            }

            // El PermisoPolicyProvider interpretará este nombre como un código de permiso
            Policy = permissionCode;
        }
    }
}
