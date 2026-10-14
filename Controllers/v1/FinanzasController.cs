using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Finanzas;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/finanzas")]
    [Authorize]
    public class FinanzasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAperturaCajaService _aperturaCajaService;
        private readonly IPagoService _pagoService;
        private readonly IAplicacionPagoService _aplicacionPagoService;
        private readonly IMovimientoCajaService _movimientoCajaService;
        private readonly IObligacionCxCRepository _obligacionRepository;

        public FinanzasController(
            ApplicationDbContext db,
            ICurrentUserService currentUser,
            IAperturaCajaService aperturaCajaService,
            IPagoService pagoService,
            IAplicacionPagoService aplicacionPagoService,
            IMovimientoCajaService movimientoCajaService,
            IObligacionCxCRepository obligacionRepository)
        {
            _db = db;
            _currentUser = currentUser;
            _aperturaCajaService = aperturaCajaService;
            _pagoService = pagoService;
            _aplicacionPagoService = aplicacionPagoService;
            _movimientoCajaService = movimientoCajaService;
            _obligacionRepository = obligacionRepository;
        }

        private int IdEmpresaActual()
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0) throw new FinanzasDomainException("El token no contiene empresa activa.");
            return idEmpresa.Value;
        }

        private int IdUsuarioActual()
        {
            if (!int.TryParse(_currentUser.IdUsuario, out var idUsuario) || idUsuario <= 0)
                throw new FinanzasDomainException("El token no contiene usuario valido.");
            return idUsuario;
        }

        [HttpGet("cajas")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ListarCajas(CancellationToken cancellationToken)
        {
            var idEmpresa = IdEmpresaActual();
            var query = _db.Cajas.AsNoTracking().Where(c => c.IdEmpresa == idEmpresa && c.Activo);

            var cajas = await query
                .OrderBy(c => c.Nombre)
                .Select(c => new CajaResponse { IdCaja = c.IdCaja, IdSucursal = c.IdSucursal, Nombre = c.Nombre, Activo = c.Activo })
                .ToListAsync(cancellationToken);

            return Ok(cajas);
        }

        [HttpGet("cajas/{idCaja:int}/apertura-activa")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerAperturaActiva(int idCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaService.ObtenerAperturaActivaAsync(IdEmpresaActual(), idCaja, cancellationToken);
            if (apertura == null) return Ok(null);
            return Ok(MapearApertura(apertura));
        }

        [HttpPost("cajas/aperturas")]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequest request, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaService.AbrirCajaAsync(IdEmpresaActual(), request.IdCaja, IdUsuarioActual(), request.SaldoInicial, cancellationToken);
            return Ok(MapearApertura(apertura));
        }

        [HttpPost("cajas/aperturas/{idApertura:int}/cerrar")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> CerrarCaja(int idApertura, [FromBody] CerrarCajaRequest request, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaService.CerrarCajaAsync(IdEmpresaActual(), idApertura, request.SaldoContado, request.Observaciones ?? string.Empty, cancellationToken);
            return Ok(MapearApertura(apertura));
        }

        [HttpGet("medios-pago")]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> ListarMediosPago(CancellationToken cancellationToken)
        {
            var medios = await _db.MediosPago.AsNoTracking()
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new MedioPagoResponse { IdMedio = m.IdMedio, Nombre = m.Nombre })
                .ToListAsync(cancellationToken);

            var canales = await _db.CanalesPago.AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new CanalPagoResponse { IdCanal = c.IdCanal, Nombre = c.Nombre })
                .ToListAsync(cancellationToken);

            var idEmpresa = IdEmpresaActual();
            var proveedores = await _db.ProveedoresPago.AsNoTracking()
                .Where(p => p.IdEmpresa == idEmpresa && p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorPagoResponse { IdProveedor = p.IdProveedor, Nombre = p.Nombre })
                .ToListAsync(cancellationToken);

            return Ok(new { medios, canales, proveedores });
        }

        [HttpPost("pagos/venta")]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> RegistrarPagoVenta([FromBody] RegistrarPagoVentaRequest request, CancellationToken cancellationToken)
        {
            var idEmpresa = IdEmpresaActual();
            var venta = await _db.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.IdEmpresa == idEmpresa && v.IdVenta == request.IdVenta, cancellationToken);
            if (venta == null) throw new FinanzasDomainException("La venta no existe o no pertenece a la empresa actual.");
            if (request.Monto <= 0) throw new FinanzasDomainException("El monto de pago debe ser mayor a cero.");

            if (!await _db.MediosPago.AnyAsync(m => m.IdMedio == request.IdMedioPago && m.Activo, cancellationToken))
                throw new FinanzasDomainException("El medio de pago no existe o no esta activo.");
            if (!await _db.CanalesPago.AnyAsync(c => c.IdCanal == request.IdCanalPago && c.Activo, cancellationToken))
                throw new FinanzasDomainException("El canal de pago no existe o no esta activo.");
            if (request.IdProveedor.HasValue && !await _db.ProveedoresPago.AnyAsync(p => p.IdEmpresa == idEmpresa && p.IdProveedor == request.IdProveedor.Value && p.Activo, cancellationToken))
                throw new FinanzasDomainException("El proveedor de pago no existe o no esta activo para la empresa actual.");

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existente = await _pagoService.BuscarPorIdempotencyKeyAsync(idEmpresa, request.IdempotencyKey, cancellationToken);
                if (existente != null)
                {
                    var app = await _db.AplicacionesPago.AsNoTracking().FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.IdPago == existente.IdPago, cancellationToken);
                    if (app == null) return Ok(new PagoVentaResponse { IdPago = existente.IdPago, Estado = existente.Estado, EstadoPago = existente.Estado });
                    return Ok(await MapearPagoVentaResponseAsync(idEmpresa, existente, app, null, cancellationToken));
                }
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var obligacion = await _db.ObligacionesCxC.FirstOrDefaultAsync(o => o.IdEmpresa == idEmpresa && o.IdVentaOrigen == venta.IdVenta, cancellationToken);
                if (obligacion == null)
                {
                    obligacion = new ObligacionCxC
                    {
                        IdEmpresa = idEmpresa,
                        IdCliente = venta.IdCliente,
                        IdVentaOrigen = venta.IdVenta,
                        Moneda = venta.Moneda,
                        DeudaOriginal = venta.Total,
                        FechaVencimiento = DateTime.Now,
                        Estado = "PENDIENTE",
                        FechaCreacion = DateTime.Now,
                        UsuarioCreacion = _currentUser.Username
                    };
                    await _obligacionRepository.AgregarAsync(obligacion, cancellationToken);
                    await _obligacionRepository.GuardarCambiosAsync(cancellationToken);
                }

                var totalAplicadoPrevio = await _db.AplicacionesPago.AsNoTracking()
                    .Where(a => a.IdEmpresa == idEmpresa && a.IdObligacion == obligacion.IdObligacion)
                    .SumAsync(a => a.MontoAplicado, cancellationToken);
                var saldoPendiente = obligacion.DeudaOriginal - totalAplicadoPrevio;
                if (saldoPendiente <= 0) throw new FinanzasDomainException("La venta ya no tiene saldo pendiente.");
                if (request.Monto > saldoPendiente) throw new FinanzasDomainException("El monto de pago excede el saldo pendiente de la venta.");

                var pago = await _pagoService.RegistrarPagoAsync(new Pago
                {
                    IdEmpresa = idEmpresa,
                    IdCliente = venta.IdCliente,
                    MontoTotal = request.Monto,
                    Moneda = request.Moneda,
                    IdMedioPago = request.IdMedioPago,
                    IdCanalPago = request.IdCanalPago,
                    IdProveedor = request.IdProveedor,
                    ReferenciaExterna = request.ReferenciaExterna,
                    IdempotencyKey = request.IdempotencyKey,
                    Adquirente = request.Adquirente,
                    CodigoAutorizacion = request.CodigoAutorizacion,
                    Ultimos4Digitos = request.Ultimos4Digitos,
                    Estado = "CONFIRMADO",
                    FechaConfirmacion = DateTime.Now
                }, cancellationToken);

                var aplicacion = await _aplicacionPagoService.AplicarPagoAsync(idEmpresa, pago.IdPago, obligacion, request.Monto, cancellationToken);

                MovimientoCaja? movimiento = null;
                if (request.IdAperturaCaja.HasValue)
                {
                    movimiento = await _movimientoCajaService.RegistrarIngresoAsync(
                        idEmpresa,
                        request.IdAperturaCaja.Value,
                        request.Monto,
                        $"VENTA:{venta.IdVenta};PAGO:{pago.IdPago}",
                        cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Ok(await MapearPagoVentaResponseAsync(idEmpresa, pago, aplicacion, movimiento, cancellationToken));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new FinanzasDomainException("Conflicto de concurrencia al registrar el pago. Vuelva a consultar el saldo e intente nuevamente.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        [HttpGet("ventas/{idVenta:int}/estado-financiero")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerEstadoFinancieroVenta(int idVenta, CancellationToken cancellationToken)
        {
            var idEmpresa = IdEmpresaActual();
            var venta = await _db.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.IdEmpresa == idEmpresa && v.IdVenta == idVenta, cancellationToken);
            if (venta == null) throw new FinanzasDomainException("La venta no existe o no pertenece a la empresa actual.");

            var estado = await ConstruirEstadoFinancieroVentaAsync(idEmpresa, venta.IdVenta, cancellationToken);
            return Ok(estado);
        }

        [HttpGet("cajas/aperturas/{idApertura:int}/movimientos")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerMovimientosCaja(int idApertura, CancellationToken cancellationToken)
        {
            var idEmpresa = IdEmpresaActual();
            var existe = await _db.AperturasCajas.AsNoTracking().AnyAsync(a => a.IdEmpresa == idEmpresa && a.IdApertura == idApertura, cancellationToken);
            if (!existe) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");

            var movimientos = await _db.MovimientosCajas.AsNoTracking()
                .Where(m => m.IdEmpresa == idEmpresa && m.IdApertura == idApertura)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new MovimientoCajaResponse
                {
                    IdMovCaja = m.IdMovCaja,
                    IdApertura = m.IdApertura,
                    Tipo = m.Tipo,
                    Monto = m.Monto,
                    IdOrigen = m.IdOrigen,
                    Fecha = m.Fecha
                })
                .ToListAsync(cancellationToken);

            return Ok(movimientos);
        }

        [HttpGet("cajas/aperturas/{idApertura:int}/resumen")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ObtenerResumenCaja(int idApertura, CancellationToken cancellationToken)
        {
            var idEmpresa = IdEmpresaActual();
            var apertura = await _db.AperturasCajas.AsNoTracking().Include(a => a.Caja)
                .FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.IdApertura == idApertura, cancellationToken);
            if (apertura == null) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");

            var movimientos = await _db.MovimientosCajas.AsNoTracking()
                .Where(m => m.IdEmpresa == idEmpresa && m.IdApertura == idApertura)
                .OrderBy(m => m.Fecha).ToListAsync(cancellationToken);
            var ingresos = movimientos.Where(m => m.Tipo == "INGRESO").Sum(m => m.Monto);
            var egresos = movimientos.Where(m => m.Tipo == "EGRESO").Sum(m => m.Monto);
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var ventasDelDia = await _db.Ventas.AsNoTracking()
                .Where(v => v.IdEmpresa == idEmpresa && v.IdSucursal == apertura.Caja.IdSucursal && v.FechaVenta >= hoy && v.FechaVenta < manana && v.Estado != "ANULADA")
                .SumAsync(v => (decimal?)v.Total, cancellationToken) ?? 0;
            var comprobantes = await _db.Comprobantes.AsNoTracking()
                .CountAsync(c => c.IdEmpresa == idEmpresa && c.IdSucursal == apertura.Caja.IdSucursal && c.FechaCreacion >= hoy && c.FechaCreacion < manana && c.EstadoProcesamiento != "ANULADO", cancellationToken);

            var desde = hoy.AddDays(-6);
            var serie = Enumerable.Range(0, 7).Select(i => desde.AddDays(i)).Select(fecha => new CajaSerieDiariaResponse
            {
                Fecha = fecha,
                Ingresos = movimientos.Where(m => m.Fecha >= fecha && m.Fecha < fecha.AddDays(1) && m.Tipo == "INGRESO").Sum(m => m.Monto),
                Egresos = movimientos.Where(m => m.Fecha >= fecha && m.Fecha < fecha.AddDays(1) && m.Tipo == "EGRESO").Sum(m => m.Monto)
            }).ToList();
            var total = ingresos + egresos;
            var distribucion = movimientos.GroupBy(m => m.Tipo).Select(g => new CajaDistribucionResponse
            {
                Tipo = g.Key,
                Monto = g.Sum(x => x.Monto),
                Porcentaje = total == 0 ? 0 : Math.Round(g.Sum(x => x.Monto) * 100m / total, 2)
            }).ToList();
            var pagosPorMedio = await _db.Pagos.AsNoTracking()
                .Where(p => p.IdEmpresa == idEmpresa && p.Estado == "CONFIRMADO" && p.FechaCreacion >= apertura.FechaApertura)
                .GroupBy(p => p.MedioPago.Nombre)
                .Select(g => new { Tipo = g.Key, Monto = g.Sum(p => p.MontoTotal) })
                .ToListAsync(cancellationToken);
            var totalPagos = pagosPorMedio.Sum(x => x.Monto);
            var distribucionMedios = pagosPorMedio.Select(x => new CajaDistribucionResponse
            {
                Tipo = x.Tipo,
                Monto = x.Monto,
                Porcentaje = totalPagos == 0 ? 0 : Math.Round(x.Monto * 100m / totalPagos, 2)
            }).OrderByDescending(x => x.Monto).ToList();

            return Ok(new CajaResumenResponse
            {
                SaldoActual = apertura.SaldoEsperado,
                TotalIngresos = ingresos,
                TotalEgresos = egresos,
                TotalMovimientos = movimientos.Count,
                VentasDelDia = ventasDelDia,
                ComprobantesDelDia = comprobantes,
                Apertura = MapearApertura(apertura),
                UltimosSieteDias = serie,
                Distribucion = distribucion,
                DistribucionMediosPago = distribucionMedios
            });
        }

        [HttpPost("cajas/aperturas/{idApertura:int}/pausar")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> PausarCaja(int idApertura, CancellationToken cancellationToken) =>
            Ok(MapearApertura(await _aperturaCajaService.PausarCajaAsync(IdEmpresaActual(), idApertura, cancellationToken)));

        [HttpPost("cajas/aperturas/{idApertura:int}/reanudar")]
        [Authorize(Policy = "VENTAS_GESTIONAR")]
        public async Task<IActionResult> ReanudarCaja(int idApertura, CancellationToken cancellationToken) =>
            Ok(MapearApertura(await _aperturaCajaService.ReanudarCajaAsync(IdEmpresaActual(), idApertura, cancellationToken)));

        [HttpPost("cajas/aperturas/{idApertura:int}/movimientos")]
        [Authorize(Policy = "VENTAS_CREAR")]
        public async Task<IActionResult> RegistrarMovimientoCaja(int idApertura, [FromBody] RegistrarMovimientoCajaRequest request, CancellationToken cancellationToken)
        {
            if (request.IdApertura != idApertura) throw new FinanzasDomainException("La apertura indicada no coincide con la solicitud.");
            var idEmpresa = IdEmpresaActual();
            var tipo = request.Tipo.Trim().ToUpperInvariant();
            MovimientoCaja movimiento = tipo is "INGRESO" or "COBRO" or "VENTA"
                ? await _movimientoCajaService.RegistrarIngresoAsync(idEmpresa, idApertura, request.Monto, $"{request.Concepto.Trim()}|{request.Referencia?.Trim()}", cancellationToken)
                : tipo is "EGRESO" or "PAGO" or "DEVOLUCION" or "DEVOLUCIÓN"
                    ? await _movimientoCajaService.RegistrarEgresoAsync(idEmpresa, idApertura, request.Monto, $"{request.Concepto.Trim()}|{request.Referencia?.Trim()}", cancellationToken)
                    : throw new FinanzasDomainException("El tipo de movimiento no es válido.");
            return Ok(new MovimientoCajaResponse { IdMovCaja = movimiento.IdMovCaja, IdApertura = movimiento.IdApertura, Tipo = movimiento.Tipo, Monto = movimiento.Monto, IdOrigen = movimiento.IdOrigen, Fecha = movimiento.Fecha });
        }

        private static AperturaCajaResponse MapearApertura(AperturaCaja apertura)
        {
            return new AperturaCajaResponse
            {
                IdApertura = apertura.IdApertura,
                IdCaja = apertura.IdCaja,
                Estado = apertura.Estado,
                SaldoInicial = apertura.SaldoInicial,
                SaldoEsperado = apertura.SaldoEsperado,
                SaldoContado = apertura.SaldoContado,
                FechaApertura = apertura.FechaApertura,
                FechaCierre = apertura.FechaCierre
            };
        }

        private async Task<PagoVentaResponse> MapearPagoVentaResponseAsync(int idEmpresa, Pago pago, AplicacionPago aplicacion, MovimientoCaja? movimiento, CancellationToken cancellationToken)
        {
            var obligacion = await _db.ObligacionesCxC.AsNoTracking()
                .FirstAsync(o => o.IdEmpresa == idEmpresa && o.IdObligacion == aplicacion.IdObligacion, cancellationToken);
            var totalAplicado = await _db.AplicacionesPago.AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdObligacion == aplicacion.IdObligacion)
                .SumAsync(a => a.MontoAplicado, cancellationToken);

            return new PagoVentaResponse
            {
                IdPago = pago.IdPago,
                IdAplicacion = aplicacion.IdAplicacion,
                IdObligacion = aplicacion.IdObligacion,
                IdMovimientoCaja = movimiento?.IdMovCaja,
                Estado = pago.Estado,
                MontoAplicado = aplicacion.MontoAplicado,
                TotalAplicado = totalAplicado,
                SaldoPendiente = Math.Max(0, obligacion.DeudaOriginal - totalAplicado),
                EstadoObligacion = obligacion.Estado,
                EstadoPago = pago.Estado
            };
        }

        private async Task<EstadoFinancieroVentaResponse> ConstruirEstadoFinancieroVentaAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken)
        {
            var venta = await _db.Ventas.AsNoTracking().FirstAsync(v => v.IdEmpresa == idEmpresa && v.IdVenta == idVenta, cancellationToken);
            var obligacion = await _db.ObligacionesCxC.AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdEmpresa == idEmpresa && o.IdVentaOrigen == idVenta, cancellationToken);

            if (obligacion == null)
            {
                return new EstadoFinancieroVentaResponse
                {
                    IdVenta = venta.IdVenta,
                    Total = venta.Total,
                    DeudaOriginal = venta.Total,
                    TotalAplicado = 0,
                    SaldoPendiente = venta.Total,
                    EstadoObligacion = "PENDIENTE"
                };
            }

            var aplicaciones = await _db.AplicacionesPago.AsNoTracking()
                .Where(a => a.IdEmpresa == idEmpresa && a.IdObligacion == obligacion.IdObligacion)
                .Include(a => a.Pago)
                    .ThenInclude(p => p.MedioPago)
                .Include(a => a.Pago)
                    .ThenInclude(p => p.CanalPago)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync(cancellationToken);

            var totalAplicado = aplicaciones.Sum(a => a.MontoAplicado);

            return new EstadoFinancieroVentaResponse
            {
                IdVenta = venta.IdVenta,
                IdObligacion = obligacion.IdObligacion,
                Total = venta.Total,
                DeudaOriginal = obligacion.DeudaOriginal,
                TotalAplicado = totalAplicado,
                SaldoPendiente = Math.Max(0, obligacion.DeudaOriginal - totalAplicado),
                EstadoObligacion = obligacion.Estado,
                Pagos = aplicaciones.Select(a => new PagoFinancieroResponse
                {
                    IdPago = a.IdPago,
                    Monto = a.MontoAplicado,
                    MedioPago = a.Pago.MedioPago.Nombre,
                    CanalPago = a.Pago.CanalPago.Nombre,
                    Estado = a.Pago.Estado,
                    Fecha = a.Pago.FechaConfirmacion ?? a.Pago.FechaCreacion
                }).ToList()
            };
        }
    }
}
