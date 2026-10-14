using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.DTOs.Compras;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras;
using Microsoft.AspNetCore.SignalR;
using SistemaVentasMotorrepuestos.API.Hubs;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ComprasController : ControllerBase
    {
        private readonly IComprasService _comprasService;
        private readonly ICurrentUserService _currentUser;
        private readonly IHubContext<ComprasHub> _comprasHub;

        public ComprasController(IComprasService comprasService, ICurrentUserService currentUser, IHubContext<ComprasHub> comprasHub)
        {
            _comprasService = comprasService;
            _currentUser = currentUser;
            _comprasHub = comprasHub;
        }

        [HttpGet]
        [Authorize(Policy = "COMPRAS_GESTIONAR")]
        public async Task<IActionResult> GetCompras([FromHeader(Name = "x-sucursal-id")] int idSucursal = 1)
        {
            var idEmpresa = _currentUser.IdEmpresa ?? 0;
            if (idEmpresa <= 0 || idSucursal <= 0) return BadRequest("Faltan identificadores de empresa o sucursal.");

            try
            {
                var compras = await _comprasService.ObtenerComprasAsync(idEmpresa, idSucursal);
                return Ok(compras);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpGet("kpis")]
        [Authorize(Policy = "COMPRAS_GESTIONAR")]
        public async Task<IActionResult> GetKpis([FromHeader(Name = "x-sucursal-id")] int idSucursal = 1)
        {
            var idEmpresa = _currentUser.IdEmpresa ?? 0;
            if (idEmpresa <= 0 || idSucursal <= 0) return BadRequest("Faltan identificadores de empresa o sucursal.");

            try
            {
                var kpis = await _comprasService.ObtenerKpisAsync(idEmpresa, idSucursal);
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "COMPRAS_GESTIONAR")]
        public async Task<IActionResult> RegistrarCompra([FromBody] NuevaCompraDTO request, [FromHeader(Name = "x-sucursal-id")] int idSucursal = 1)
        {
            var idEmpresa = _currentUser.IdEmpresa ?? 0;
            int.TryParse(_currentUser.IdUsuario, out int idUsuario);
            if (idEmpresa <= 0 || idSucursal <= 0 || idUsuario <= 0) return BadRequest("Faltan identificadores de usuario/empresa/sucursal.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var compra = await _comprasService.RegistrarCompraAsync(idEmpresa, idSucursal, idUsuario, request);
                await _comprasHub.Clients.Group(ComprasHub.GrupoEmpresa(idEmpresa)).SendAsync("comprasActualizadas", new { tipo = "registrada", idCompra = compra.IdCompra });
                return Ok(compra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("{idCompra}/estado")]
        [Authorize(Policy = "COMPRAS_GESTIONAR")]
        public async Task<IActionResult> CambiarEstadoCompra(
            int idCompra,
            [FromBody] CambioEstadoCompraDTO request)
        {
            int.TryParse(_currentUser.IdUsuario, out int idUsuario);
            if (idUsuario <= 0) return BadRequest("Falta identificador de usuario.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var compra = await _comprasService.CambiarEstadoCompraAsync(idCompra, request, idUsuario);
                var idEmpresa = _currentUser.IdEmpresa ?? 0;
                await _comprasHub.Clients.Group(ComprasHub.GrupoEmpresa(idEmpresa)).SendAsync("comprasActualizadas", new { tipo = "estado", idCompra = compra.IdCompra, estado = compra.Estado });
                return Ok(compra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
