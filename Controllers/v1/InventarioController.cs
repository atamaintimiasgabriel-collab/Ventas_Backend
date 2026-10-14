using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Inventario;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Autorizacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using Microsoft.AspNetCore.SignalR;
using SistemaVentasMotorrepuestos.API.Hubs;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IRepositorioBase<Stock> _stockRepository;
        private readonly IRepositorioBase<Almacen> _almacenRepository;
        private readonly IRepositorioBase<Producto> _productoRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext _dbContext;
        private readonly IHubContext<InventarioHub> _inventarioHub;

        public InventarioController(
            IRepositorioBase<Stock> stockRepository,
            IRepositorioBase<Almacen> almacenRepository,
            IRepositorioBase<Producto> productoRepository,
            ICurrentUserService currentUser,
            SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext dbContext,
            IHubContext<InventarioHub> inventarioHub)
        {
            _stockRepository = stockRepository;
            _almacenRepository = almacenRepository;
            _productoRepository = productoRepository;
            _currentUser = currentUser;
            _dbContext = dbContext;
            _inventarioHub = inventarioHub;
        }

        /// <summary>
        /// Obtiene la empresa actualmente autenticada desde el contexto de seguridad.
        /// Lanza excepciÃ³n si el usuario no tiene una empresa vÃ¡lida asignada.
        /// </summary>
        private int ObtenerEmpresaActual()
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                throw new UnauthorizedAccessException("El usuario autenticado no tiene una empresa asignada o el token es invÃ¡lido.");
            return idEmpresa.Value;
        }

        /// <summary>
        /// Valida que un almacÃ©n especÃ­fico pertenece a la empresa actualmente autenticada.
        /// Lanza BadRequest si la validaciÃ³n falla (almacÃ©n no existe o pertenece a otra empresa).
        /// </summary>
        private async Task<bool> ValidarAlmacenPerteneceAEmpresaAsync(int idAlmacen, CancellationToken cancellationToken = default)
        {
            var idEmpresaActual = ObtenerEmpresaActual();
            var almacenes = await _almacenRepository.BuscarAsync(
                a => a.IdAlmacen == idAlmacen,
                cancellationToken);
            
            var almacen = almacenes.FirstOrDefault();
            if (almacen == null)
                return false; // AlmacÃ©n no existe
            
            // Validar que el almacÃ©n pertenece a la empresa actual (mediante Sucursal)
            if (almacen.Sucursal?.IdEmpresa != idEmpresaActual)
                return false; // AlmacÃ©n pertenece a otra empresa

            return true; // ValidaciÃ³n exitosa
        }

        /// <summary>
        /// GET /api/v1/inventario
        /// Lista informaciÃ³n de stock del inventario filtrado por empresa autenticada
        /// ValidaciÃ³n obligatoria: idAlmacen debe pertenecer a la empresa activa
        /// </summary>
        [HttpGet]
        [RequireTenantPermission("INVENTARIO_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StockResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListarInventario(
            [FromQuery] int? idAlmacen = null,
            [FromQuery] int? idProducto = null,
            [FromQuery] string? estadoStock = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var idEmpresaActual = ObtenerEmpresaActual();

                // VALIDACIÃ“N CRÃTICA: Si idAlmacen es especificado, validar que pertenece a la empresa actual
                if (idAlmacen.HasValue)
                {
                    var esAlmacenValido = await ValidarAlmacenPerteneceAEmpresaAsync(idAlmacen.Value, cancellationToken);
                    if (!esAlmacenValido)
                    {
                        return BadRequest(new { mensaje = $"El almacÃ©n {idAlmacen} no pertenece a la empresa autenticada o no existe." });
                    }
                }

                var stocks = await _dbContext.Stocks
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(s => s.Producto).ThenInclude(p => p!.Categoria)
                    .Include(s => s.Producto).ThenInclude(p => p!.Marca)
                    .Include(s => s.Ubicacion).ThenInclude(u => u!.Almacen).ThenInclude(a => a!.Sucursal)
                    .Where(s => s.Ubicacion!.Almacen!.Sucursal!.IdEmpresa == idEmpresaActual)
                    .ToListAsync(cancellationToken);

                // Filtrar por almacÃ©n si se especifica (despuÃ©s de validaciÃ³n)
                if (idAlmacen.HasValue)
                {
                    stocks = stocks.Where(s => s.IdAlmacen == idAlmacen.Value).ToList();
                }

                // Filtrar por producto especÃ­fico
                if (idProducto.HasValue)
                {
                    stocks = stocks.Where(s => s.IdProducto == idProducto.Value).ToList();
                }

                // Filtrar por estado de stock
                if (!string.IsNullOrWhiteSpace(estadoStock))
                {
                    stocks = stocks.Where(s =>
                    {
                        var disponible = s.StockFisico - s.StockReservado - s.StockSeguridad;
                        return estadoStock.ToUpper() switch
                        {
                            "DISPONIBLE" => disponible > s.StockMinimo,
                            "BAJO" => disponible > 0 && disponible <= s.StockMinimo,
                            "AGOTADO" => disponible <= 0,
                            _ => true
                        };
                    }).ToList();
                }

                var response = stocks.Select(s => new StockResponse
                {
                    IdStock = s.IdStock,
                    IdProducto = s.IdProducto,
                    SKUProducto = s.Producto?.SKU ?? "",
                    NombreProducto = s.Producto?.NombreComercial ?? "",
                    IdAlmacen = s.IdAlmacen,
                    NombreAlmacen = s.Ubicacion?.Almacen?.Nombre ?? "",
                    IdUbicacion = s.IdUbicacion,
                    CodigoUbicacion = s.Ubicacion?.Codigo ?? "",
                    StockFisico = s.StockFisico,
                    StockReservado = s.StockReservado,
                    StockSeguridad = s.StockSeguridad,
                    StockMinimo = s.StockMinimo
                }).OrderBy(s => s.NombreAlmacen).ThenBy(s => s.NombreProducto).ToList();

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener inventario", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/inventario/resumen
        /// Obtiene resumen agregado de inventario por producto (empresa actual)
        /// ValidaciÃ³n: idAlmacen debe pertenecer a empresa autenticada
        /// </summary>
        [HttpGet("resumen")]
        [RequireTenantPermission("INVENTARIO_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventarioResumenResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerResumenInventario(
            [FromQuery] int? idAlmacen = null,
            [FromQuery] string? estadoStock = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var idEmpresaActual = ObtenerEmpresaActual();

                // VALIDACIÃ“N CRÃTICA: Si idAlmacen es especificado, validar que pertenece a la empresa actual
                if (idAlmacen.HasValue)
                {
                    var esAlmacenValido = await ValidarAlmacenPerteneceAEmpresaAsync(idAlmacen.Value, cancellationToken);
                    if (!esAlmacenValido)
                    {
                        return BadRequest(new { mensaje = $"El almacÃ©n {idAlmacen} no pertenece a la empresa autenticada o no existe." });
                    }
                }

                var stocks = await _dbContext.Stocks
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(s => s.Producto).ThenInclude(p => p!.Categoria)
                    .Include(s => s.Producto).ThenInclude(p => p!.Marca)
                    .Include(s => s.Ubicacion).ThenInclude(u => u!.Almacen).ThenInclude(a => a!.Sucursal)
                    .Where(s => s.Producto != null && s.Ubicacion!.Almacen!.Sucursal!.IdEmpresa == idEmpresaActual)
                    .ToListAsync(cancellationToken);

                if (idAlmacen.HasValue)
                {
                    stocks = stocks.Where(s => s.IdAlmacen == idAlmacen.Value).ToList();
                }

                // Agrupar por producto
                var gruposProducto = stocks.Where(s => s.Producto != null).GroupBy(s => s.IdProducto);

                var response = gruposProducto.Select(grupo =>
                {
                    var producto = grupo.First().Producto!;
                    var totalDisponible = grupo.Sum(s => s.StockFisico - s.StockReservado - s.StockSeguridad);
                    var totalReservado = grupo.Sum(s => s.StockReservado);
                    var minimoGrupo = grupo.Max(s => s.StockMinimo);

                    var estado = totalDisponible <= 0 ? "AGOTADO" :
                                 totalDisponible <= minimoGrupo ? "BAJO" : "DISPONIBLE";

                    return new InventarioResumenResponse
                    {
                        IdProducto = producto.IdProducto,
                        SKU = producto.SKU,
                        NombreProducto = producto.NombreComercial,
                        NomeCategoria = producto.Categoria?.Nombre ?? "",
                        NombreMarca = producto.Marca?.Nombre ?? "",
                        StockTotalDisponible = totalDisponible,
                        StockTotalReservado = totalReservado,
                        StockMinimo = minimoGrupo,
                        EstadoStock = estado,
                        CantidadUbicaciones = grupo.Count()
                    };
                }).ToList();

                // Filtrar por estado si se especifica
                if (!string.IsNullOrWhiteSpace(estadoStock))
                {
                    response = response.Where(r => r.EstadoStock.Equals(estadoStock, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(response.OrderBy(r => r.NombreProducto).ToList());
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener resumen", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/inventario/almacenes
        /// Obtiene lista de almacenes autorizados para la empresa autenticada
        /// Filtro obligatorio: solo almacenes cuya sucursal pertenece a la empresa
        /// </summary>
        [HttpGet("almacenes")]
        [RequireTenantPermission("INVENTARIO_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerAlmacenes(CancellationToken cancellationToken = default)
        {
            try
            {
                var idEmpresaActual = ObtenerEmpresaActual();

                // Filtrar almacenes que pertenecen a la empresa actual
                // AlmacÃ©n â†’ Sucursal â†’ IdEmpresa
                var almacenes = await _almacenRepository.BuscarAsync(
                    a => a.Activo && a.Sucursal != null && a.Sucursal.IdEmpresa == idEmpresaActual,
                    cancellationToken);
                
                var response = almacenes.Select(a => new { IdAlmacen = a.IdAlmacen, Nombre = a.Nombre })
                    .OrderBy(a => a.Nombre).ToList();

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener almacenes", error = ex.Message });
            }
        }

        [HttpGet("movimientos-hoy")]
        [RequireTenantPermission("INVENTARIO_CONSULTAR")]
        public async Task<IActionResult> ObtenerMovimientosDeHoy(CancellationToken cancellationToken = default)
        {
            var idEmpresaActual = ObtenerEmpresaActual();
            var inicio = DateTime.UtcNow.Date;
            var fin = inicio.AddDays(1);
            var cantidad = await _dbContext.MovimientosInventario
                .Include(m => m.Ubicacion).ThenInclude(u => u!.Almacen).ThenInclude(a => a!.Sucursal)
                .Where(m => m.FechaHora >= inicio && m.FechaHora < fin &&
                            m.Ubicacion!.Almacen!.Sucursal!.IdEmpresa == idEmpresaActual)
                .CountAsync(cancellationToken);
            return Ok(new { cantidad });
        }

        /// <summary>
        /// GET /api/v1/inventario/{idProducto}
        /// Obtiene detalle de stock de un producto (solo en almacenes de empresa autenticada)
        /// </summary>
        [HttpGet("{idProducto}")]
        [RequireTenantPermission("INVENTARIO_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StockResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerStockProducto(int idProducto, CancellationToken cancellationToken = default)
        {
            try
            {
                var idEmpresaActual = ObtenerEmpresaActual();

                var producto = await _productoRepository.ObtenerPorIdAsync(idProducto);
                if (producto == null)
                {
                    return NotFound(new { mensaje = $"Producto {idProducto} no encontrado" });
                }

                var stocks = await _dbContext.Stocks
                    .AsNoTracking()
                    .Include(s => s.Producto)
                    .Include(s => s.Ubicacion).ThenInclude(u => u!.Almacen).ThenInclude(a => a!.Sucursal)
                    .Where(s => s.IdProducto == idProducto && s.Ubicacion!.Almacen!.Sucursal!.IdEmpresa == idEmpresaActual)
                    .ToListAsync(cancellationToken);

                // Filtrar solo stock en almacenes de la empresa autenticada
                stocks = stocks.Where(s => s.Ubicacion?.Almacen?.Sucursal?.IdEmpresa == idEmpresaActual).ToList();

                var response = stocks.Select(s => new StockResponse
                {
                    IdStock = s.IdStock,
                    IdProducto = s.IdProducto,
                    SKUProducto = s.Producto?.SKU ?? "",
                    NombreProducto = s.Producto?.NombreComercial ?? "",
                    IdAlmacen = s.IdAlmacen,
                    NombreAlmacen = s.Ubicacion?.Almacen?.Nombre ?? "",
                    IdUbicacion = s.IdUbicacion,
                    CodigoUbicacion = s.Ubicacion?.Codigo ?? "",
                    StockFisico = s.StockFisico,
                    StockReservado = s.StockReservado,
                    StockSeguridad = s.StockSeguridad,
                    StockMinimo = s.StockMinimo
                }).ToList();

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener stock del producto", error = ex.Message });
            }
        }

        [HttpPost("ajuste")]
        [RequireTenantPermission("INVENTARIO_MODIFICAR")]
        public async Task<IActionResult> AjusteStock([FromBody] AjusteStockRequest request, [FromServices] SistemaVentasMotorrepuestos.API.Datos.ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            try
            {
                var idEmpresaActual = ObtenerEmpresaActual();
                if (request.Cantidad <= 0) return BadRequest(new { mensaje = "La cantidad debe ser mayor a 0" });
                
                var stock = await dbContext.Stocks.FindAsync(new object[] { request.IdStock }, cancellationToken);
                if (stock == null) return NotFound(new { mensaje = "Stock no encontrado." });
                
                // Fetch Almacen to check permissions
                var almacen = await dbContext.Almacenes.Include(a => a.Sucursal).FirstOrDefaultAsync(a => a.IdAlmacen == stock.IdAlmacen, cancellationToken);
                if (almacen == null || almacen.Sucursal?.IdEmpresa != idEmpresaActual)
                    return BadRequest(new { mensaje = "El stock pertenece a un almacén de otra empresa." });

                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                
                decimal saldoAnterior = stock.StockFisico;
                decimal saldoActual = saldoAnterior;

                if (request.Tipo == "SUMAR") saldoActual += request.Cantidad;
                else if (request.Tipo == "RESTAR") saldoActual -= request.Cantidad;
                else if (request.Tipo == "REEMPLAZAR") saldoActual = request.Cantidad;
                else return BadRequest(new { mensaje = "Tipo de ajuste no válido." });

                if (saldoActual < 0) return BadRequest(new { mensaje = "El saldo no puede ser negativo." });

                stock.StockFisico = saldoActual;

                // Create Movimiento
                var mov = new MovimientoInventario
                {
                    IdProducto = stock.IdProducto,
                    IdAlmacen = stock.IdAlmacen,
                    IdUbicacion = stock.IdUbicacion,
                    IdTipoMovimiento = 5, // Assuming 5 is 'Ajuste de Inventario'
                    Cantidad = saldoActual - saldoAnterior,
                    SaldoAnterior = saldoAnterior,
                    SaldoActual = saldoActual,
                    FechaHora = DateTime.UtcNow,
                    UsuarioResponsable = _currentUser.IdUsuario ?? "SISTEMA",
                    MotivoObservacion = request.Motivo
                };

                dbContext.MovimientosInventario.Add(mov);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _inventarioHub.Clients.Group(InventarioHub.GrupoEmpresa(idEmpresaActual))
                    .SendAsync("inventarioActualizado", new { idStock = stock.IdStock, stockActual = stock.StockFisico }, cancellationToken);

                return Ok(new { mensaje = "Stock ajustado correctamente.", stockActual = stock.StockFisico });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno.", error = ex.Message });
            }
        }
    }
}

