using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaVentasMotorrepuestos.API.Controllers
{
    // [TEST ONLY] Controlador temporal para validar las políticas dinámicas de la Fase 1.4.4.7
    // SERÁ ELIMINADO ANTES DE FINALIZAR LA TAREA.
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestOnlyAuthorizationController : ControllerBase
    {
        [HttpGet("inventario/consultar")]
        [Authorize(Policy = "INVENTARIO_CONSULTAR")]
        public IActionResult ConsultarInventario()
        {
            return Ok(new { mensaje = "Acceso concedido a consultar inventario" });
        }

        [HttpGet("venta/crear")]
        [Authorize(Policy = "VENTA_CREAR")]
        public IActionResult CrearVenta()
        {
            return Ok(new { mensaje = "Acceso concedido a crear venta" });
        }

        [HttpGet("dinamico/permisoprueba")]
        [Authorize(Policy = "PERMISO_PRUEBA")]
        public IActionResult PermisoPrueba()
        {
            return Ok(new { mensaje = "Acceso concedido a PERMISO_PRUEBA" });
        }

        [HttpGet("dinamico/permisoinactivo")]
        [Authorize(Policy = "PERMISO_INACTIVO")]
        public IActionResult PermisoInactivo()
        {
            return Ok(new { mensaje = "Acceso concedido a PERMISO_INACTIVO" });
        }
    }
}
