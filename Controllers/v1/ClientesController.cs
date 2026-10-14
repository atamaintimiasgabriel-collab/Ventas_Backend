using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.DTOs.Clientes;
using SistemaVentasMotorrepuestos.API.Servicios.Autorizacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    /// <summary>
    /// ClientesController: REST API para consulta de clientes
    /// Endpoints protegidos con tenant context y permisos
    /// Tenant isolation: IdEmpresa extraído automáticamente del contexto autenticado
    /// No acepta IdEmpresa como parámetro desde cliente
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>
        /// GET /api/v1/clientes
        /// Lista clientes de la empresa autenticada con filtros opcionales
        /// Requiere: CLIENTES_CONSULTAR + AccessToken válido
        /// La empresa activa se extrae automáticamente del contexto autenticado
        /// </summary>
        [HttpGet]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteListResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListarClientes(
            [FromQuery] ClienteFilterRequest filtro,
            CancellationToken cancellationToken = default)
        {
            var result = await _clienteService.ListarClientesAsync(filtro, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/clientes/{id}
        /// Obtiene detalle completo de un cliente específico
        /// Requiere: CLIENTES_CONSULTAR + AccessToken válido
        /// Validación: El cliente debe pertenecera la empresa autenticada
        /// </summary>
        [HttpGet("{id}")]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClienteDetailResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerClientePorId(
            int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _clienteService.ObtenerClientePorIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("venta/default")]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> ObtenerClienteDefaultParaVenta(CancellationToken cancellationToken)
        {
            var clientes = await _clienteService.ListarClientesAsync(new ClienteFilterRequest
            {
                Buscar = "Cliente General",
                Activo = true,
                PageSize = 50
            }, cancellationToken);
            var cliente = clientes.FirstOrDefault();
            if (cliente == null)
            {
                // Fallback seguro: el Cajero recibe solo un cliente activo para completar la venta,
                // nunca el catálogo general de clientes.
                var activos = await _clienteService.ListarClientesAsync(new ClienteFilterRequest
                {
                    Activo = true,
                    PageNumber = 1,
                    PageSize = 1
                }, cancellationToken);
                cliente = activos.FirstOrDefault();
            }
            return cliente == null ? NotFound(new { mensaje = "No existe un cliente general activo para ventas." }) : Ok(cliente);
        }

        [HttpPost]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClienteDetailResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CrearCliente(
            [FromBody] CrearClienteRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _clienteService.CrearClienteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(ObtenerClientePorId), new { id = result.IdCliente }, result);
        }

        [HttpGet("estadisticas")]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        public async Task<IActionResult> Estadisticas(CancellationToken cancellationToken = default) => Ok(await _clienteService.ObtenerEstadisticasAsync(cancellationToken));

        [HttpGet("paginados")]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        public async Task<IActionResult> Paginados([FromQuery] ClienteFilterRequest filtro, CancellationToken cancellationToken = default) => Ok(await _clienteService.ListarClientesPaginadosAsync(filtro, cancellationToken));

        [HttpPut("{id}")]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteRequest request, CancellationToken cancellationToken = default) => Ok(await _clienteService.ActualizarClienteAsync(id, request, cancellationToken));

        [HttpPatch("{id}/estado")]
        [RequireTenantPermission("CLIENTES_CONSULTAR")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] bool activo, CancellationToken cancellationToken = default) => Ok(await _clienteService.CambiarEstadoAsync(id, activo, cancellationToken));
    }
}
