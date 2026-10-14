using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaVentasMotorrepuestos.API.Configuracion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    // Implementación del servicio de generación JWT utilizando estándares criptográficos.
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public string GenerateToken(string idUsuario, string username, int? idEmpresa = null, int? idSucursal = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, idUsuario),
                new Claim(ClaimTypes.NameIdentifier, idUsuario),
                new Claim(ClaimTypes.Name, username)
            };

            if (idEmpresa.HasValue)
            {
                claims.Add(new Claim("IdEmpresa", idEmpresa.Value.ToString()));
                claims.Add(new Claim("id_empresa", idEmpresa.Value.ToString()));
            }

            if (idSucursal.HasValue)
            {
                claims.Add(new Claim("IdSucursal", idSucursal.Value.ToString()));
                claims.Add(new Claim("id_sucursal", idSucursal.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
