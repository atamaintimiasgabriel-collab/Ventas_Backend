using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class PermisoAuthorizationService : IPermisoAuthorizationService
    {
        private readonly ApplicationDbContext _dbContext;

        public PermisoAuthorizationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> UsuarioTienePermisoAsync(int idUsuario, string codigoPermiso, CancellationToken cancellationToken = default)
        {
            // Verificamos que el usuario esté activo, que tenga al menos un rol activo
            // y que al menos uno de esos roles tenga el permiso activo correspondiente al codigo.
            return await _dbContext.Usuarios
                .AsNoTracking()
                .Where(u => u.IdUsuario == idUsuario && u.Activo)
                .SelectMany(u => u.UsuariosRoles)
                .Select(ur => ur.Rol)
                .Where(r => r.Activo)
                .SelectMany(r => r.RolesPermisos)
                .Select(rp => rp.Permiso)
                .Where(p => p.Activo)
                .AnyAsync(p => p.CodigoSistema == codigoPermiso
                    || (codigoPermiso == "PRODUCTOS_CONSULTAR" && p.CodigoSistema == "PRODUCTOS_GESTIONAR"), cancellationToken);
        }
    }
}
