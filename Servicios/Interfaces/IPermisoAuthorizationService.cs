namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IPermisoAuthorizationService
    {
        Task<bool> UsuarioTienePermisoAsync(int idUsuario, string codigoPermiso, CancellationToken cancellationToken = default);
    }
}
