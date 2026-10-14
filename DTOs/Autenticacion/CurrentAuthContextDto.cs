namespace SistemaVentasMotorrepuestos.API.DTOs.Autenticacion
{
    /// <summary>
    /// Respuesta GET /api/v1/auth/me
    /// Retorna el contexto actual de autenticación del usuario incluyendo
    /// usuario, empresa activa, sucursal activa, lista de empresas/sucursales disponibles,
    /// roles y permisos efectivos.
    /// </summary>
    public class CurrentAuthContextDto
    {
        public AuthUserDto Usuario { get; set; } = new();
        public AuthorizedCompanyDto? EmpresaActiva { get; set; }
        public AuthorizedBranchDto? SucursalActiva { get; set; }
        public List<AuthorizedCompanyDto> Empresas { get; set; } = new();
        public List<AuthorizedBranchDto> Sucursales { get; set; } = new();
        public List<EffectiveRoleDto> Roles { get; set; } = new();
        public List<string> Permisos { get; set; } = new();
    }

    public class AuthUserDto
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class AuthorizedBranchDto
    {
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class EffectiveRoleDto
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
