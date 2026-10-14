using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.DTOs.Ventas;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpPost]
        [Authorize(Policy = "VENTAS_CREAR")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VentaResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Idempotencia o concurrencia Pedido
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                request.IdempotencyKey = idempotencyKey;

            var result = await _ventaService.CrearVentaAsync(request, cancellationToken);
            
            return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = result.IdVenta }, result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "VENTAS_GESTIONAR")] // o VENTAS_VER
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerVentaPorId(int id, CancellationToken cancellationToken)
        {
            var result = await _ventaService.ObtenerVentaPorIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("numero/{numeroVenta}")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerVentaPorNumero(string numeroVenta, CancellationToken cancellationToken)
        {
            var result = await _ventaService.ObtenerVentaPorNumeroAsync(numeroVenta, cancellationToken);
            return Ok(result);
        }

        [HttpGet("pedido/{idPedido}")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VentaResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerVentaPorPedido(int idPedido, CancellationToken cancellationToken)
        {
            var result = await _ventaService.ObtenerVentaPorPedidoAsync(idPedido, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VentaListResponse>))]
        public async Task<IActionResult> ListarVentas([FromQuery] VentaFilterRequest filtro, CancellationToken cancellationToken)
        {
            var result = await _ventaService.ListarVentasAsync(filtro, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id}/anular")]
        [Authorize(Policy = "VENTAS_ANULAR")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Concurrency Optimista (RowVersion)
        public async Task<IActionResult> AnularVenta(int id, [FromBody] AnularVentaRequest request, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken cancellationToken)
        {
            byte[]? rowVersion = null;
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                try { rowVersion = Convert.FromBase64String(ifMatch.Replace("\"", "")); }
                catch { return BadRequest(new { Message = "If-Match header is invalid base64." }); }
            }

            if (rowVersion == null || rowVersion.Length == 0)
                return BadRequest(new { Message = "If-Match header (RowVersion) is required." });

            await _ventaService.AnularVentaAsync(id, request.Motivo, rowVersion, cancellationToken);

            return NoContent();
        }
    }
}
