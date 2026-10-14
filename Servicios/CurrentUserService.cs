using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    // Implementación concreta que lee la identidad actual desde HttpContext.
    // Centralizado para que si la estrategia de Claims cambia (ej. JWT),
    // solo se modifique aquí y el resto de la arquitectura siga funcionando.
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Recupera el IdUsuario (comúnmente almacenado en el NameIdentifier de JWT)
        public string? IdUsuario => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Recupera el Username (comúnmente Name o UniqueName)
        public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        
        // Recupera el IdEmpresa desde el claim personalizado "IdEmpresa"
        public int? IdEmpresa 
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("IdEmpresa");
                return int.TryParse(claim, out var id) ? id : null;
            }
        }
        
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
