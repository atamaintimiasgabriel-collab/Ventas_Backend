using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/comprobantes")]
    [Authorize]
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobanteService _comprobanteService;
        private readonly ICurrentUserService _currentUser;
        private readonly IDocumentoImpresionService _documentoImpresion;
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _fileStorage;

        public ComprobantesController(IComprobanteService comprobanteService, ICurrentUserService currentUser, IDocumentoImpresionService documentoImpresion, ApplicationDbContext db, IFileStorageService fileStorage)
        {
            _comprobanteService = comprobanteService;
            _currentUser = currentUser;
            _documentoImpresion = documentoImpresion;
            _db = db;
            _fileStorage = fileStorage;
        }

        /// <summary>Directorio paginado y métricas del módulo. Los filtros se aplican en SQL y siempre se aíslan por empresa.</summary>
        [HttpGet]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> Listar([FromQuery] ComprobanteListadoFiltro filtro, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var page = Math.Max(1, filtro.PageNumber);
            var pageSize = Math.Clamp(filtro.PageSize, 1, 100);
            var consulta = _db.Comprobantes.AsNoTracking()
                .Include(c => c.Venta!).ThenInclude(v => v.Cliente)
                .Where(c => c.IdEmpresa == idEmpresa.Value);

            if (!string.IsNullOrWhiteSpace(filtro.TipoComprobante))
                consulta = consulta.Where(c => c.TipoComprobante == filtro.TipoComprobante);
            if (!string.IsNullOrWhiteSpace(filtro.Estado))
            {
                consulta = filtro.Estado switch
                {
                    "Emitido" => consulta.Where(c => c.EstadoFiscal == "ACEPTADO"),
                    "Anulado" => consulta.Where(c => c.EstadoFiscal == "ANULADO"),
                    "Rechazado" => consulta.Where(c => c.EstadoFiscal == "RECHAZADO"),
                    "Pendiente" => consulta.Where(c => c.EstadoFiscal != "ACEPTADO" && c.EstadoFiscal != "ANULADO" && c.EstadoFiscal != "RECHAZADO"),
                    _ => consulta.Where(c => c.EstadoFiscal == filtro.Estado || c.EstadoProcesamiento == filtro.Estado)
                };
            }
            if (filtro.Desde.HasValue)
                consulta = consulta.Where(c => c.FechaCreacion >= filtro.Desde.Value.Date);
            if (filtro.Hasta.HasValue)
                consulta = consulta.Where(c => c.FechaCreacion < filtro.Hasta.Value.Date.AddDays(1));
            if (filtro.IdCliente.HasValue)
                consulta = consulta.Where(c => c.Venta != null && c.Venta.IdCliente == filtro.IdCliente.Value);
            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var term = filtro.Buscar.Trim();
                consulta = consulta.Where(c => c.SerieFiscal.Contains(term)
                    || (c.Venta != null && c.Venta.NumeroVenta.Contains(term))
                    || (c.Venta != null && c.Venta.Cliente != null && ((c.Venta.Cliente.RazonSocial ?? "").Contains(term)
                        || (c.Venta.Cliente.Nombres ?? "").Contains(term)
                        || (c.Venta.Cliente.Apellidos ?? "").Contains(term)
                        || c.Venta.Cliente.NumeroDocumento.Contains(term))));
            }

            var total = await consulta.CountAsync(cancellationToken);
            var registros = await consulta.OrderByDescending(c => c.FechaCreacion)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var todos = await consulta.ToListAsync(cancellationToken);

            return Ok(new ComprobanteListadoResponse
            {
                Items = registros.Select(MapearListado).ToList(),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Resumen = CrearResumen(todos)
            });
        }

        /// <summary>Documento imprimible (01 factura, 03 boleta o TICKET) con los datos reales de la venta.</summary>
        [HttpGet("venta/{idVenta:int}/documento")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerDocumento(int idVenta, [FromQuery] string tipo, [FromQuery] decimal? recibido, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var documento = await _documentoImpresion.ObtenerDocumentoAsync(idEmpresa.Value, idVenta, tipo, recibido, cancellationToken);
            return Ok(documento);
        }

        [HttpGet("venta/{idVenta:int}/pdf")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerPdf(int idVenta, [FromQuery] string tipo, [FromQuery] decimal? recibido, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var (contenido, nombre) = await _documentoImpresion.GenerarPdfAsync(idEmpresa.Value, idVenta, tipo, recibido, cancellationToken);
            Response.Headers["X-Nombre-Archivo"] = nombre;
            return File(contenido, "application/pdf", nombre);
        }

        [HttpPost]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> Crear([FromBody] CrearComprobanteRequest request, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var comprobante = await _comprobanteService.CrearComprobanteAsync(
                idEmpresa.Value,
                request.IdVenta,
                request.TipoComprobante,
                request.SerieFiscal ?? string.Empty,
                _currentUser.Username ?? "SISTEMA",
                request.IdempotencyKey ?? $"COMPROBANTE:{request.IdVenta}:{request.TipoComprobante}:{request.SerieFiscal}",
                cancellationToken);

            await _comprobanteService.SolicitarProcesamientoAsync(comprobante.IdComprobante, idEmpresa.Value, cancellationToken);

            return Ok(Mapear(comprobante));
        }

        [HttpGet("{idComprobante:guid}")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> Obtener(Guid idComprobante, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var comprobante = await _comprobanteService.ConsultarEstadoAsync(idComprobante, idEmpresa.Value, cancellationToken);
            if (comprobante == null) return NotFound(new { mensaje = "Comprobante no encontrado." });

            return Ok(Mapear(comprobante));
        }

        [HttpGet("{idComprobante:guid}/xml")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public Task<IActionResult> DescargarXml(Guid idComprobante, CancellationToken cancellationToken) => DescargarArtefacto(idComprobante, "xml", cancellationToken);

        [HttpGet("{idComprobante:guid}/cdr")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public Task<IActionResult> DescargarCdr(Guid idComprobante, CancellationToken cancellationToken) => DescargarArtefacto(idComprobante, "cdr", cancellationToken);

        private async Task<IActionResult> DescargarArtefacto(Guid idComprobante, string tipo, CancellationToken cancellationToken)
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                return BadRequest(new { mensaje = "El token no contiene empresa activa." });

            var comprobante = await _comprobanteService.ConsultarEstadoAsync(idComprobante, idEmpresa.Value, cancellationToken);
            if (comprobante == null) return NotFound(new { mensaje = "Comprobante no encontrado." });
            var key = tipo == "xml" ? comprobante.StorageKeyXml : comprobante.StorageKeyCdr;
            if (string.IsNullOrWhiteSpace(key))
                return NotFound(new { mensaje = $"El comprobante no tiene {tipo.ToUpperInvariant()} disponible." });

            var contenido = await _fileStorage.ObtenerArchivoAsync(key, cancellationToken);
            if (contenido == null) return NotFound(new { mensaje = "El artefacto registrado no existe en el almacenamiento." });
            var extension = Path.GetExtension(key);
            var mediaType = tipo == "xml" ? "application/xml" : "application/zip";
            return File(contenido, mediaType, $"{comprobante.SerieFiscal}-{comprobante.CorrelativoFiscal:D8}{extension}");
        }

        private static ComprobanteResponse Mapear(Modelos.Comprobante comprobante)
        {
            return new ComprobanteResponse
            {
                IdComprobante = comprobante.IdComprobante,
                IdVenta = comprobante.IdVenta,
                TipoComprobante = comprobante.TipoComprobante,
                SerieFiscal = comprobante.SerieFiscal,
                CorrelativoFiscal = comprobante.CorrelativoFiscal,
                EstadoProcesamiento = comprobante.EstadoProcesamiento,
                EstadoFiscal = comprobante.EstadoFiscal,
                StorageKeyXml = comprobante.StorageKeyXml,
                StorageKeyCdr = comprobante.StorageKeyCdr,
                StorageKeyPdf = comprobante.StorageKeyPdf
            };
        }

        private static ComprobanteListadoItem MapearListado(Modelos.Comprobante c)
        {
            var cliente = c.Venta?.Cliente;
            var nombreCliente = cliente?.TipoPersona == "J"
                ? cliente.RazonSocial
                : string.Join(" ", new[] { cliente?.Nombres, cliente?.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x)));
            return new ComprobanteListadoItem
            {
                IdComprobante = c.IdComprobante,
                IdVenta = c.IdVenta,
                FechaEmision = c.FechaCreacion,
                TipoComprobante = c.TipoComprobante,
                TipoDescripcion = TipoDescripcion(c.TipoComprobante),
                SerieCorrelativo = c.CorrelativoFiscal.HasValue ? $"{c.SerieFiscal}-{c.CorrelativoFiscal.Value:D8}" : c.SerieFiscal,
                Cliente = string.IsNullOrWhiteSpace(nombreCliente) ? "Cliente no disponible" : nombreCliente,
                Total = c.Venta?.Total ?? 0,
                Moneda = c.Venta?.Moneda ?? "PEN",
                EstadoFiscal = c.EstadoFiscal,
                EstadoProcesamiento = c.EstadoProcesamiento,
                EstadoPresentacion = EstadoPresentacion(c.EstadoFiscal, c.EstadoProcesamiento),
                TieneXml = !string.IsNullOrWhiteSpace(c.StorageKeyXml),
                TieneCdr = !string.IsNullOrWhiteSpace(c.StorageKeyCdr),
                TienePdf = !string.IsNullOrWhiteSpace(c.StorageKeyPdf)
            };
        }

        private static ComprobanteResumen CrearResumen(IEnumerable<Modelos.Comprobante> comprobantes) => new()
        {
            Total = comprobantes.Count(),
            Facturas = comprobantes.Count(c => c.TipoComprobante is "01" or "FACTURA"),
            Boletas = comprobantes.Count(c => c.TipoComprobante is "03" or "BOLETA"),
            NotasCredito = comprobantes.Count(c => c.TipoComprobante is "07" or "NOTA_CREDITO"),
            NotasDebito = comprobantes.Count(c => c.TipoComprobante is "08" or "NOTA_DEBITO"),
            PorTipo = comprobantes.GroupBy(c => TipoDescripcion(c.TipoComprobante)).ToDictionary(g => g.Key, g => g.Count()),
            PorEstado = comprobantes.GroupBy(c => EstadoPresentacion(c.EstadoFiscal, c.EstadoProcesamiento)).ToDictionary(g => g.Key, g => g.Count()),
            PorMes = comprobantes.GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new ComprobanteMes { Periodo = $"{g.Key.Year:D4}-{g.Key.Month:D2}", Facturas = g.Count(c => c.TipoComprobante is "01" or "FACTURA"), Boletas = g.Count(c => c.TipoComprobante is "03" or "BOLETA"), Notas = g.Count(c => c.TipoComprobante is "07" or "08" or "NOTA_CREDITO" or "NOTA_DEBITO") }).TakeLast(6).ToList()
        };

        private static string TipoDescripcion(string tipo) => tipo switch { "01" or "FACTURA" => "Factura", "03" or "BOLETA" => "Boleta", "07" or "NOTA_CREDITO" => "Nota Crédito", "08" or "NOTA_DEBITO" => "Nota Débito", _ => tipo };
        private static string EstadoPresentacion(string fiscal, string procesamiento) => fiscal switch { "ACEPTADO" => "Emitido", "ANULADO" => "Anulado", "RECHAZADO" => "Rechazado", "OBSERVADO" => "Observado", _ when procesamiento == "FINALIZADO" => fiscal, _ => "Pendiente" };
    }

    public class ComprobanteListadoFiltro { public string? Buscar { get; set; } public string? TipoComprobante { get; set; } public string? Estado { get; set; } public DateTime? Desde { get; set; } public DateTime? Hasta { get; set; } public int? IdCliente { get; set; } public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; }
    public class ComprobanteListadoResponse { public List<ComprobanteListadoItem> Items { get; set; } = []; public int PageNumber { get; set; } public int PageSize { get; set; } public int TotalCount { get; set; } public int TotalPages { get; set; } public ComprobanteResumen Resumen { get; set; } = new(); }
    public class ComprobanteListadoItem { public Guid IdComprobante { get; set; } public int? IdVenta { get; set; } public DateTime FechaEmision { get; set; } public string TipoComprobante { get; set; } = string.Empty; public string TipoDescripcion { get; set; } = string.Empty; public string SerieCorrelativo { get; set; } = string.Empty; public string Cliente { get; set; } = string.Empty; public decimal Total { get; set; } public string Moneda { get; set; } = "PEN"; public string EstadoFiscal { get; set; } = string.Empty; public string EstadoProcesamiento { get; set; } = string.Empty; public string EstadoPresentacion { get; set; } = string.Empty; public bool TieneXml { get; set; } public bool TieneCdr { get; set; } public bool TienePdf { get; set; } }
    public class ComprobanteResumen { public int Total { get; set; } public int Facturas { get; set; } public int Boletas { get; set; } public int NotasCredito { get; set; } public int NotasDebito { get; set; } public Dictionary<string, int> PorTipo { get; set; } = []; public Dictionary<string, int> PorEstado { get; set; } = []; public List<ComprobanteMes> PorMes { get; set; } = []; }
    public class ComprobanteMes { public string Periodo { get; set; } = string.Empty; public int Facturas { get; set; } public int Boletas { get; set; } public int Notas { get; set; } }

    public class CrearComprobanteRequest
    {
        [Range(1, int.MaxValue)]
        public int IdVenta { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoComprobante { get; set; } = "03";

        [StringLength(10)]
        public string? SerieFiscal { get; set; }

        [StringLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class ComprobanteResponse
    {
        public Guid IdComprobante { get; set; }
        public int? IdVenta { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string SerieFiscal { get; set; } = string.Empty;
        public long? CorrelativoFiscal { get; set; }
        public string EstadoProcesamiento { get; set; } = string.Empty;
        public string EstadoFiscal { get; set; } = string.Empty;
        public string? StorageKeyXml { get; set; }
        public string? StorageKeyCdr { get; set; }
        public string? StorageKeyPdf { get; set; }
    }
}
