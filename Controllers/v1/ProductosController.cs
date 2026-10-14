using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Productos;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Autorizacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Datos;
using Microsoft.EntityFrameworkCore;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IRepositorioBase<Producto> _productoRepository;
        private readonly IRepositorioBase<Categoria> _categoriaRepository;
        private readonly IRepositorioBase<Marca> _marcaRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ApplicationDbContext _context;

        public ProductosController(
            IRepositorioBase<Producto> productoRepository,
            IRepositorioBase<Categoria> categoriaRepository,
            IRepositorioBase<Marca> marcaRepository,
            ICurrentUserService currentUser,
            ApplicationDbContext context)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
            _marcaRepository = marcaRepository;
            _currentUser = currentUser;
            _context = context;
        }

        /// <summary>
        /// GET /api/v1/productos
        /// Lista todos los productos del catálogo activos
        /// Requiere permiso: PRODUCTOS_GESTIONAR (acceso a datos comerciales incluyendo costo)
        /// </summary>
        [HttpGet]
        [RequireTenantPermission("PRODUCTOS_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoListResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListarProductos([FromQuery] string? buscar = null, [FromQuery] int? idCategoria = null, [FromQuery] int? idMarca = null)
        {
            try
            {
                var query = _context.Productos.AsNoTracking()
                    .Where(p => p.Estado == "ACTIVO" || string.IsNullOrEmpty(p.Estado));

                // Filtrar por búsqueda
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    query = query.Where(p =>
                        p.SKU.Contains(buscar) ||
                        p.NombreComercial.Contains(buscar) ||
                        (p.CodigoOEM != null && p.CodigoOEM.Contains(buscar)) ||
                        (p.CodigoBarras != null && p.CodigoBarras.Contains(buscar))
                    );
                }

                // Filtrar por categoría
                if (idCategoria.HasValue)
                {
                    query = query.Where(p => p.IdCategoria == idCategoria.Value);
                }

                // Filtrar por marca
                if (idMarca.HasValue)
                {
                    query = query.Where(p => p.IdMarca == idMarca.Value);
                }

                var productos = await query.ToListAsync();

                var categorias = await _context.Categorias.AsNoTracking().ToDictionaryAsync(c => c.IdCategoria, c => c.Nombre);
                var marcas = await _context.Marcas.AsNoTracking().ToDictionaryAsync(m => m.IdMarca, m => m.Nombre);

                // Stock de los almacenes de la sucursal activa del token (o de la empresa si no hay sucursal).
                var idEmpresaActual = _currentUser.IdEmpresa;
                int? idSucursalActual = int.TryParse(User.FindFirst("IdSucursal")?.Value ?? User.FindFirst("id_sucursal")?.Value, out var sid) ? sid : null;
                var stockPorProducto = await _context.Stocks.AsNoTracking()
                    .Where(s => _context.Almacenes.Any(a => a.IdAlmacen == s.IdAlmacen
                        && (idSucursalActual != null
                            ? a.IdSucursal == idSucursalActual
                            : _context.Sucursales.Any(su => su.IdSucursal == a.IdSucursal && su.IdEmpresa == idEmpresaActual))))
                    .GroupBy(s => s.IdProducto)
                    .Select(g => new { g.Key, Total = g.Sum(x => x.StockDisponible ?? 0m) })
                    .ToDictionaryAsync(x => x.Key, x => x.Total);

                var response = productos.Select(p => new ProductoListResponse
                {
                    IdProducto = p.IdProducto,
                    SKU = p.SKU,
                    CodigoBarras = p.CodigoBarras,
                    CodigoOEM = p.CodigoOEM,
                    NombreComercial = p.NombreComercial,
                    IdCategoria = p.IdCategoria,
                    NombreCategoria = p.Categoria?.Nombre ?? categorias.GetValueOrDefault(p.IdCategoria, ""),
                    IdMarca = p.IdMarca,
                    NombreMarca = p.Marca?.Nombre ?? marcas.GetValueOrDefault(p.IdMarca, ""),
                    IdUnidadMedida = p.IdUnidadMedida,
                    CodigoUnidadMedida = p.UnidadMedida?.Codigo ?? "",
                    PrecioVenta = p.PrecioVenta,
                    PrecioCompra = p.PrecioCompra,
                    Estado = p.Estado,
                    RutaImagen = p.RutaImagen,
                    StockDisponible = (int)Math.Floor(stockPorProducto.GetValueOrDefault(p.IdProducto, 0m))
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener productos", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/productos/{id}
        /// Obtiene detalle de un producto específico
        /// Requiere permiso: PRODUCTOS_GESTIONAR
        /// </summary>
        [HttpGet("{id}")]
        [RequireTenantPermission("PRODUCTOS_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductoListResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ObtenerProducto(int id)
        {
            try
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(id);
                if (producto == null)
                {
                    return NotFound(new { mensaje = $"Producto {id} no encontrado" });
                }

                var response = new ProductoListResponse
                {
                    IdProducto = producto.IdProducto,
                    SKU = producto.SKU,
                    CodigoBarras = producto.CodigoBarras,
                    CodigoOEM = producto.CodigoOEM,
                    NombreComercial = producto.NombreComercial,
                    IdCategoria = producto.IdCategoria,
                    NombreCategoria = producto.Categoria?.Nombre ?? "",
                    IdMarca = producto.IdMarca,
                    NombreMarca = producto.Marca?.Nombre ?? "",
                    IdUnidadMedida = producto.IdUnidadMedida,
                    CodigoUnidadMedida = producto.UnidadMedida?.Codigo ?? "",
                    PrecioVenta = producto.PrecioVenta,
                    PrecioCompra = producto.PrecioCompra,
                    Estado = producto.Estado,
                    RutaImagen = producto.RutaImagen
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener producto", error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/v1/productos
        /// </summary>
        [HttpPost]
        [RequireTenantPermission("PRODUCTOS_GESTIONAR")]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoRequest request)
        {
            try
            {
                var p = new Producto
                {
                    SKU = request.SKU,
                    CodigoBarras = request.CodigoBarras,
                    CodigoOEM = request.CodigoOEM,
                    NombreComercial = request.NombreComercial,
                    DescripcionTecnica = request.DescripcionTecnica,
                    IdCategoria = request.IdCategoria,
                    IdSubcategoria = request.IdSubcategoria,
                    IdMarca = request.IdMarca,
                    IdUnidadMedida = request.IdUnidadMedida,
                    PrecioCompra = request.PrecioCompra,
                    PrecioVenta = request.PrecioVenta,
                    MargenGanancia = request.MargenGanancia,
                    Estado = request.Estado ?? "ACTIVO",
                    RutaImagen = request.RutaImagen,
                    ClasificacionTributaria = request.ClasificacionTributaria,
                    EsOferta = request.EsOferta,
                    PrecioOferta = request.PrecioOferta,
                    EsNuevoIngreso = request.EsNuevoIngreso,
                    EsRecomendado = request.EsRecomendado,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = _currentUser.IdUsuario?.ToString()
                };

                await _productoRepository.AgregarAsync(p);
                await _productoRepository.GuardarCambiosAsync();
                return Ok(new { mensaje = "Producto creado correctamente", idProducto = p.IdProducto });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear producto", error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/v1/productos/{id}
        /// </summary>
        [HttpPut("{id}")]
        [RequireTenantPermission("PRODUCTOS_GESTIONAR")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoRequest request)
        {
            try
            {
                var p = await _productoRepository.ObtenerPorIdAsync(id);
                if (p == null) return NotFound(new { mensaje = "Producto no encontrado" });

                p.SKU = request.SKU;
                p.CodigoBarras = request.CodigoBarras;
                p.CodigoOEM = request.CodigoOEM;
                p.NombreComercial = request.NombreComercial;
                p.DescripcionTecnica = request.DescripcionTecnica;
                p.IdCategoria = request.IdCategoria;
                p.IdSubcategoria = request.IdSubcategoria;
                p.IdMarca = request.IdMarca;
                p.IdUnidadMedida = request.IdUnidadMedida;
                p.PrecioCompra = request.PrecioCompra;
                p.PrecioVenta = request.PrecioVenta;
                p.MargenGanancia = request.MargenGanancia;
                p.Estado = request.Estado ?? "ACTIVO";
                p.RutaImagen = request.RutaImagen;
                p.ClasificacionTributaria = request.ClasificacionTributaria;
                p.EsOferta = request.EsOferta;
                p.PrecioOferta = request.PrecioOferta;
                p.EsNuevoIngreso = request.EsNuevoIngreso;
                p.EsRecomendado = request.EsRecomendado;
                p.FechaModificacion = DateTime.UtcNow;
                p.UsuarioModificacion = _currentUser.IdUsuario?.ToString();

                _productoRepository.Actualizar(p);
                await _productoRepository.GuardarCambiosAsync();
                return Ok(new { mensaje = "Producto actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al actualizar producto", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/productos/catalogo
        /// Lista productos del catálogo para el cliente público con filtros
        /// </summary>
        [HttpGet("catalogo")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoListResponse>))]
        public async Task<IActionResult> CatalogoProductos([FromQuery] string filtro = "mas-vendidos", [FromQuery] int limite = 8, [FromQuery] int? idCategoria = null)
        {
            try
            {
                var query = await _productoRepository.BuscarAsync(p => p.Estado == "ACTIVO" || string.IsNullOrEmpty(p.Estado));
                // Includes para stock
                var queryWithIncludes = _context.Productos
                    .Include(p => p.Marca)
                    .Include(p => p.Stocks)
                    .Where(p => p.Estado == "ACTIVO" || string.IsNullOrEmpty(p.Estado));
                
                IEnumerable<Producto> productosQuery = queryWithIncludes;

                if (idCategoria.HasValue)
                {
                    productosQuery = productosQuery.Where(p => p.IdCategoria == idCategoria.Value);
                }

                switch (filtro?.ToLower())
                {
                    case "ofertas":
                        productosQuery = productosQuery.Where(p => p.EsOferta).OrderBy(p => p.IdProducto);
                        break;
                    case "nuevos":
                        productosQuery = productosQuery.Where(p => p.EsNuevoIngreso).OrderByDescending(p => p.IdProducto);
                        break;
                    case "recomendados":
                        productosQuery = productosQuery.Where(p => p.EsRecomendado).OrderByDescending(p => p.IdProducto);
                        break;
                    case "mas-vendidos":
                    default:
                        productosQuery = productosQuery.OrderBy(p => p.IdProducto);
                        break;
                }
                
                var response = productosQuery
                    .Take(limite)
                    .Select(p => new ProductoListResponse
                    {
                        IdProducto = p.IdProducto,
                        SKU = p.SKU,
                        NombreComercial = p.NombreComercial,
                        NombreMarca = p.Marca?.Nombre ?? "",
                        PrecioVenta = p.PrecioVenta,
                        RutaImagen = p.RutaImagen,
                        DescripcionTecnica = p.DescripcionTecnica,
                        StockDisponible = (int)(p.Stocks.Sum(s => s.StockDisponible ?? 0)),
                        EsOferta = p.EsOferta,
                        PrecioOferta = p.PrecioOferta,
                        EsNuevoIngreso = p.EsNuevoIngreso,
                        EsRecomendado = p.EsRecomendado
                    }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener catálogo", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/productos/buscar
        /// Busca productos por vehiculo, codigo OEM o nombre
        /// </summary>
        [HttpGet("buscar")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoListResponse>))]
        public async Task<IActionResult> BuscarProductos(
            [FromQuery] string? nombre = null,
            [FromQuery] string? codigoOem = null,
            [FromQuery] int? idMarcaVehiculo = null,
            [FromQuery] int? idModeloVehiculo = null,
            [FromQuery] int? anio = null,
            [FromQuery] string? cilindrada = null,
            [FromQuery] int? idCategoria = null)
        {
            try
            {
                var query = _context.Productos
                    .Include(p => p.Marca)
                    .Include(p => p.Stocks)
                    .Where(p => p.Estado == "ACTIVO" || string.IsNullOrEmpty(p.Estado))
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(p => p.NombreComercial.Contains(nombre) || p.SKU.Contains(nombre));
                }

                if (!string.IsNullOrWhiteSpace(codigoOem))
                {
                    query = query.Where(p => p.CodigoOEM != null && p.CodigoOEM.Contains(codigoOem));
                }

                if (idCategoria.HasValue)
                {
                    query = query.Where(p => p.IdCategoria == idCategoria.Value);
                }

                // Filtrar por vehículo si se envían parámetros
                if (idMarcaVehiculo.HasValue || idModeloVehiculo.HasValue)
                {
                    var subQueryCompatibilidad = _context.CompatibilidadesProducto
                        .Include(c => c.VersionVehiculo)
                        .ThenInclude(v => v.ModeloVehiculo)
                        .AsQueryable();

                    if (idMarcaVehiculo.HasValue)
                    {
                        subQueryCompatibilidad = subQueryCompatibilidad.Where(c => c.VersionVehiculo.ModeloVehiculo.IdMarcaVehiculo == idMarcaVehiculo.Value);
                    }

                    if (idModeloVehiculo.HasValue)
                    {
                        subQueryCompatibilidad = subQueryCompatibilidad.Where(c => c.VersionVehiculo.IdModeloVehiculo == idModeloVehiculo.Value);
                    }

                    if (anio.HasValue)
                    {
                        subQueryCompatibilidad = subQueryCompatibilidad.Where(c => 
                            (!c.VersionVehiculo.AñoInicio.HasValue || c.VersionVehiculo.AñoInicio.Value <= anio.Value) &&
                            (!c.VersionVehiculo.AñoFin.HasValue || c.VersionVehiculo.AñoFin.Value >= anio.Value)
                        );
                    }

                    if (!string.IsNullOrWhiteSpace(cilindrada))
                    {
                        // Some systems store it on the Model, some on the version. According to ModeloVehiculo.cs, it's on the Model.
                        subQueryCompatibilidad = subQueryCompatibilidad.Where(c => c.VersionVehiculo.ModeloVehiculo.Cilindrada == cilindrada);
                    }

                    var productIdsCompatibles = await subQueryCompatibilidad.Select(c => c.IdProducto).Distinct().ToListAsync();
                    query = query.Where(p => productIdsCompatibles.Contains(p.IdProducto));
                }

                var result = await query.Take(50).Select(p => new ProductoListResponse
                {
                    IdProducto = p.IdProducto,
                    SKU = p.SKU,
                    NombreComercial = p.NombreComercial,
                    NombreMarca = p.Marca != null ? p.Marca.Nombre : "",
                    PrecioVenta = p.PrecioVenta,
                    RutaImagen = p.RutaImagen,
                    DescripcionTecnica = p.DescripcionTecnica,
                    StockDisponible = (int)(p.Stocks.Sum(s => s.StockDisponible ?? 0)),
                    EsOferta = p.EsOferta,
                    PrecioOferta = p.PrecioOferta,
                    EsNuevoIngreso = p.EsNuevoIngreso,
                    EsRecomendado = p.EsRecomendado
                }).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error en la búsqueda", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/productos/categorias
        /// Obtiene lista de categorías activas para filtros
        /// </summary>
        [HttpGet("categorias/disponibles")]
        [RequireTenantPermission("PRODUCTOS_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoriaResponse>))]
        public async Task<IActionResult> ObtenerCategorias()
        {
            try
            {
                var categorias = await _categoriaRepository.BuscarAsync(c => c.Activo);
                var response = categorias.Select(c => new CategoriaResponse
                {
                    IdCategoria = c.IdCategoria,
                    Nombre = c.Nombre,
                    Activo = c.Activo
                }).OrderBy(c => c.Nombre).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener categorías", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v1/productos/marcas/disponibles
        /// Obtiene lista de marcas activas para filtros
        /// </summary>
        [HttpGet("marcas/disponibles")]
        [RequireTenantPermission("PRODUCTOS_CONSULTAR")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MarcaResponse>))]
        public async Task<IActionResult> ObtenerMarcas()
        {
            try
            {
                var marcas = await _marcaRepository.BuscarAsync(m => m.Activo);
                var response = marcas.Select(m => new MarcaResponse
                {
                    IdMarca = m.IdMarca,
                    Nombre = m.Nombre,
                    Activo = m.Activo
                }).OrderBy(m => m.Nombre).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener marcas", error = ex.Message });
            }
        }
    }
}
