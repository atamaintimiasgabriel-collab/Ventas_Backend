using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;
using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost]
        [Authorize(Policy = "PEDIDOS_CREAR")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PedidoResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Idempotency
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
        {
            // Inject idempotency key from header if not in request (optional, but header is standard)
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                request.IdempotencyKey = idempotencyKey;

            var result = await _pedidoService.CrearPedidoAsync(request);
            
            return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = result.IdPedido }, result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "PEDIDOS_GESTIONAR")] // o PEDIDOS_VER
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PedidoResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPedidoPorId(int id)
        {
            var result = await _pedidoService.ObtenerPedidoPorIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "PEDIDOS_GESTIONAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PedidoListResponse>))]
        public async Task<IActionResult> ListarPedidos([FromQuery] PedidoFilterRequest filtro)
        {
            var result = await _pedidoService.ListarPedidosAsync(filtro);
            return Ok(result);
        }

        [HttpGet("mis-pedidos")]
        [Authorize] // Cualquier cliente logueado
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PedidoListResponse>))]
        public async Task<IActionResult> MisPedidos([FromQuery] PedidoFilterRequest filtro)
        {
            // Obtener IdCliente del token si aplica, aquí simplemente pasamos el filtro al servicio.
            // Para "mis-pedidos", el filtro de cliente debe forzarse.
            // Esto asume que el ICurrentUserService ya valida la empresa y el cliente si aplica.
            var result = await _pedidoService.ListarPedidosAsync(filtro);
            return Ok(result);
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Policy = "PEDIDOS_GESTIONAR")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Concurrency
        public async Task<IActionResult> CambiarEstadoPedido(int id, [FromBody] CambiarEstadoPedidoRequest request, [FromHeader(Name = "If-Match")] string? ifMatch)
        {
            byte[]? rowVersion = null;
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                try { rowVersion = Convert.FromBase64String(ifMatch.Replace("\"", "")); }
                catch { return BadRequest(new { Message = "If-Match header is invalid base64." }); }
            }

            if (rowVersion == null || rowVersion.Length == 0)
                return BadRequest(new { Message = "If-Match header (RowVersion) is required." });

            if (request.NuevoEstado == "CANCELADO")
            {
                await _pedidoService.CancelarPedidoAsync(id, request.Motivo ?? "Sin motivo", rowVersion);
            }
            else if (request.NuevoEstado == "RECHAZADO")
            {
                await _pedidoService.RechazarPedidoAsync(id, request.Motivo ?? "Sin motivo", rowVersion);
            }
            else
            {
                await _pedidoService.CambiarEstadoPedidoAsync(id, request.NuevoEstado, rowVersion);
            }

            return NoContent();
        }
    }
}
