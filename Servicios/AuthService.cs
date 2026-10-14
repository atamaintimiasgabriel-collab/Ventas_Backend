using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaVentasMotorrepuestos.API.Configuracion;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Autenticacion;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using System.Security.Claims;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly IRepositorioBase<Usuario> _usuarioRepositorio;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly JwtOptions _jwtOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        public AuthService(
            IRepositorioBase<Usuario> usuarioRepositorio,
            IPasswordHasherService passwordHasher,
            IJwtService jwtService,
            IOptions<JwtOptions> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _jwtOptions = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            var usuarios = await _usuarioRepositorio.BuscarAsync(u => u.Username == request.Username);
            var usuario = usuarios.FirstOrDefault();

            if (usuario == null || !usuario.Activo)
            {
                return null;
            }

            bool isValid = _passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash);
            if (!isValid)
            {
                return null;
            }

            var empresa = await ObtenerEmpresaActivaAsync();
            var sucursal = empresa == null
                ? null
                : await _dbContext.Sucursales.AsNoTracking()
                    .Where(s => s.IdEmpresa == empresa.IdEmpresa && s.Activo)
                    .OrderBy(s => s.IdSucursal)
                    .FirstOrDefaultAsync();

            var token = _jwtService.GenerateToken(
                usuario.IdUsuario.ToString(),
                usuario.Username,
                empresa?.IdEmpresa,
                sucursal?.IdSucursal);

            return new LoginResponseDto
            {
                Status = "Authenticated",
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                Username = usuario.Username,
                Empresas = empresa == null
                    ? new()
                    : new() { new AuthorizedCompanyDto { IdEmpresa = empresa.IdEmpresa, RazonSocial = empresa.RazonSocial } }
            };
        }

        public async Task<CurrentAuthContextDto?> GetCurrentContextAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !(user.Identity?.IsAuthenticated ?? false))
                return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
                return null;

            var usuario = (await _usuarioRepositorio.BuscarAsync(u => u.IdUsuario == idUsuario)).FirstOrDefault();
            if (usuario == null)
                return null;

            var idEmpresa = ReadIntClaim(user, "IdEmpresa") ?? ReadIntClaim(user, "id_empresa");
            var empresa = idEmpresa.HasValue
                ? await _dbContext.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.IdEmpresa == idEmpresa.Value && e.Activo)
                : await ObtenerEmpresaActivaAsync();

            var sucursales = empresa == null
                ? new List<AuthorizedBranchDto>()
                : await _dbContext.Sucursales.AsNoTracking()
                    .Where(s => s.IdEmpresa == empresa.IdEmpresa && s.Activo)
                    .OrderBy(s => s.Nombre)
                    .Select(s => new AuthorizedBranchDto { IdSucursal = s.IdSucursal, Nombre = s.Nombre })
                    .ToListAsync();

            var idSucursal = ReadIntClaim(user, "IdSucursal") ?? ReadIntClaim(user, "id_sucursal");
            var sucursalActiva = idSucursal.HasValue
                ? sucursales.FirstOrDefault(s => s.IdSucursal == idSucursal.Value)
                : sucursales.FirstOrDefault();

            var roles = await _dbContext.Set<UsuarioRol>().AsNoTracking()
                .Where(ur => ur.IdUsuario == idUsuario && ur.Rol.Activo)
                .Select(ur => new EffectiveRoleDto { IdRol = ur.Rol.IdRol, NombreRol = ur.Rol.NombreRol })
                .Distinct()
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            var permisos = await (from ur in _dbContext.UsuariosRoles
                                  join r in _dbContext.Roles on ur.IdRol equals r.IdRol
                                  join rp in _dbContext.RolesPermisos on r.IdRol equals rp.IdRol
                                  join p in _dbContext.Permisos on rp.IdPermiso equals p.IdPermiso
                                  where ur.IdUsuario == idUsuario && r.Activo && p.Activo
                                  select p.CodigoSistema)
                                  .Distinct()
                                  .OrderBy(p => p)
                                  .ToListAsync();

            return new CurrentAuthContextDto
            {
                Usuario = new AuthUserDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Username = usuario.Username
                },
                EmpresaActiva = empresa == null ? null : new AuthorizedCompanyDto { IdEmpresa = empresa.IdEmpresa, RazonSocial = empresa.RazonSocial },
                SucursalActiva = sucursalActiva,
                Empresas = empresa == null ? new() : new() { new AuthorizedCompanyDto { IdEmpresa = empresa.IdEmpresa, RazonSocial = empresa.RazonSocial } },
                Sucursales = sucursales,
                Roles = roles,
                Permisos = permisos
            };
        }

        private async Task<Empresa?> ObtenerEmpresaActivaAsync()
        {
            return await _dbContext.Empresas.AsNoTracking()
                .Where(e => e.Activo)
                .OrderBy(e => e.IdEmpresa)
                .FirstOrDefaultAsync();
        }

        private static int? ReadIntClaim(ClaimsPrincipal user, string claimType)
        {
            var value = user.FindFirst(claimType)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
