using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SistemaVentasMotorrepuestos.API.DTOs.Autenticacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous] // Endpoint público
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Validaciones implícitas en .NET 10 mediante DataAnnotations (devuelve 400 Bad Request si faltan campos).
            
            var response = await _authService.AuthenticateAsync(request);

            if (response == null)
            {
                // Devolvemos 401 unificado. No se revela "Usuario incorrecto" vs "Clave incorrecta" 
                // para evitar enumeración de usuarios.
                return Unauthorized(new { mensaje = "Credenciales inválidas." });
            }

            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize] // Requiere AccessToken válido
        [EnableRateLimiting("GlobalPolicy")]
        public async Task<IActionResult> GetCurrentContext()
        {
            // Obtiene el contexto de autenticación actual del usuario autenticado
            // De momento, retorna estructura minimal (sin empresa/sucursal seleccionada)
            // porque el schema actual no tiene Usuario→Empresa relationship
            
            var context = await _authService.GetCurrentContextAsync();
            if (context == null)
                return NotFound(new { mensaje = "No se pudo cargar el contexto de autenticación." });

            return Ok(context);
        }
    }
}
