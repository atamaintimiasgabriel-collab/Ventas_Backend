using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.DTOs.Compras;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController, Route("api/v1/[controller]"), Authorize]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedoresService _service;
        private readonly ICurrentUserService _user;
        public ProveedoresController(IProveedoresService service, ICurrentUserService user) { _service = service; _user = user; }
        private int Empresa() => _user.IdEmpresa is > 0 ? _user.IdEmpresa.Value : throw new UnauthorizedAccessException("El usuario no tiene empresa asignada.");
        [HttpGet, Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Listar([FromQuery] ProveedorFiltroDTO filtro) => Ok(await _service.ListarAsync(Empresa(), filtro));
        [HttpGet("estadisticas"), Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Estadisticas() => Ok(await _service.ObtenerEstadisticasAsync(Empresa()));
        [HttpGet("{id:int}"), Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Obtener(int id) { var result = await _service.ObtenerProveedorPorIdAsync(Empresa(), id); return result == null ? NotFound() : Ok(result); }
        [HttpGet("{id:int}/compras"), Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Compras(int id) => Ok(await _service.ObtenerComprasAsync(Empresa(), id));
        [HttpPost, Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Crear(CrearProveedorDTO request) { var result = await _service.CrearAsync(Empresa(), request); return CreatedAtAction(nameof(Obtener), new { id = result.IdProveedor }, result); }
        [HttpPut("{id:int}"), Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Actualizar(int id, ActualizarProveedorDTO request) => Ok(await _service.ActualizarAsync(Empresa(), id, request));
        [HttpPatch("{id:int}/estado"), Authorize(Policy = "PROVEEDORES_CONSULTAR")] public async Task<IActionResult> Estado(int id, [FromBody] bool activo) => Ok(await _service.CambiarEstadoAsync(Empresa(), id, activo));
    }
}
