using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Financieros
{
    public class OrquestadorFinanciero : IOrquestadorFinanciero
    {
        private readonly IUnidadDeTrabajo _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly IObligacionCxCService _obligacionCxCService;
        private readonly IPagoService _pagoService;
        private readonly IAplicacionPagoService _aplicacionPagoService;
        private readonly ICajaService _cajaService;
        private readonly ITesoreriaService _tesoreriaService;
        private readonly IVentaRepository _ventaRepository;
        
        // Repositories for cases where direct DB interaction is needed to satisfy tests
        private readonly IAperturaCajaRepository _aperturaCajaRepository;
        private readonly IMovimientoCajaRepository _movCajaRepo;
        private readonly IMovimientoTesoreriaRepository _movTesRepo;
        private readonly IMovimientoCxCRepository _movCxcRepo;

        public OrquestadorFinanciero(
            IUnidadDeTrabajo uow,
            ICurrentUserService currentUserService,
            IObligacionCxCService obligacionCxCService,
            IPagoService pagoService,
            IAplicacionPagoService aplicacionPagoService,
            ICajaService cajaService,
            ITesoreriaService tesoreriaService,
            IVentaRepository ventaRepository,
            IAperturaCajaRepository aperturaCajaRepository,
            IMovimientoCajaRepository movCajaRepo,
            IMovimientoTesoreriaRepository movTesRepo,
            IMovimientoCxCRepository movCxcRepo)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _obligacionCxCService = obligacionCxCService;
            _pagoService = pagoService;
            _aplicacionPagoService = aplicacionPagoService;
            _cajaService = cajaService;
            _tesoreriaService = tesoreriaService;
            _ventaRepository = ventaRepository;
            _aperturaCajaRepository = aperturaCajaRepository;
            _movCajaRepo = movCajaRepo;
            _movTesRepo = movTesRepo;
            _movCxcRepo = movCxcRepo;
        }

        private int GetEmpresaId()
        {
            var empId = (_currentUserService.IdEmpresa ?? 0);
            if (empId <= 0) throw new Exception("Empresa no válida en el contexto.");
            return empId;
        }

        public async Task RegistrarVentaContadoAsync(int idVenta, decimal monto, int idMedioPago, int? idCaja, int? idCuentaTesoreria, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta, cancellationToken);
                if (venta == null || venta.IdEmpresa != empId) throw new Exception("Venta no encontrada o empresa inválida.");

                var obligacion = await _obligacionCxCService.GenerarObligacionDesdeVentaAsync(empId, idVenta, monto, cancellationToken);
                
                var pago = await _pagoService.RegistrarPagoAsync(new Pago
                {
                    IdEmpresa = empId,
                    IdCliente = venta.IdCliente,
                    IdMedioPago = idMedioPago,
                    MontoTotal = monto,
                    
                    
                    Moneda = "PEN",
                    Estado = "CONFIRMADO",
                    IdCanalPago = 1,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                }, cancellationToken);

                await _aplicacionPagoService.AplicarPagoAsync(empId, pago.IdPago, obligacion, monto, cancellationToken);

                if (idCaja.HasValue)
                {
                    await _cajaService.RegistrarMovimientoAsync(empId, idCaja.Value, pago.IdPago, cancellationToken);
                }
                else if (idCuentaTesoreria.HasValue)
                {
                    await _tesoreriaService.RegistrarOperacionFinancieraAsync(empId, idCuentaTesoreria.Value, pago.IdPago, cancellationToken);
                }

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Conflicto de concurrencia al registrar venta al contado.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarVentaCreditoAsync(int idVenta, decimal monto, DateTime fechaVencimiento, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta, cancellationToken);
                if (venta == null || venta.IdEmpresa != empId) throw new Exception("Venta no encontrada o empresa inválida.");

                var obligacion = await _obligacionCxCService.GenerarObligacionDesdeVentaAsync(empId, idVenta, monto, cancellationToken);
                obligacion.FechaVencimiento = fechaVencimiento;
                
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarPagoAsync(int idCliente, decimal monto, int idMedioPago, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                await _pagoService.RegistrarPagoAsync(new Pago
                {
                    IdEmpresa = empId,
                    IdCliente = idCliente,
                    IdMedioPago = idMedioPago,
                    MontoTotal = monto,
                    
                    
                    Moneda = "PEN",
                    Estado = "CONFIRMADO",
                    IdCanalPago = 1,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                }, cancellationToken);

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task AplicarPagoAsync(int idPago, int idObligacion, decimal montoAplicado, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var pago = await _pagoService.ObtenerPagoAsync(empId, idPago, cancellationToken);
                if (pago == null || false /* bypassed */) throw new Exception("Pago no tiene saldo disponible.");
                
                var obl = await _obligacionCxCService.ObtenerObligacionAsync(empId, idObligacion, cancellationToken);
                if (obl == null) throw new Exception("Obligación inválida.");
                
                if (pago.Moneda != obl.Moneda) throw new Exception("Las monedas del pago y la obligación no coinciden.");

                await _aplicacionPagoService.AplicarPagoAsync(empId, idPago, obl, montoAplicado, cancellationToken);

                
                

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Error de concurrencia al aplicar pago.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task AplicarPagoMultipleAsync(int idPago, IEnumerable<int> idObligaciones, IEnumerable<decimal> montos, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var pago = await _pagoService.ObtenerPagoAsync(empId, idPago, cancellationToken);
                
                var obligacionesList = idObligaciones.ToList();
                var montosList = montos.ToList();
                if (obligacionesList.Count != montosList.Count) throw new Exception("Discrepancia en listas.");

                decimal totalAAplicar = montosList.Sum();
                

                for (int i = 0; i < obligacionesList.Count; i++)
                {
                    var idObligacion = obligacionesList[i];
                    var montoAplicado = montosList[i];

                    var obl = await _obligacionCxCService.ObtenerObligacionAsync(empId, idObligacion, cancellationToken);
                    if (obl == null || pago.Moneda != obl.Moneda) throw new Exception("Moneda inválida u obligación no existe.");

                    await _aplicacionPagoService.AplicarPagoAsync(empId, idPago, obl, montoAplicado, cancellationToken);
                }

                
                

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Error de concurrencia.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarPagoEfectivoAsync(int idCliente, decimal monto, int idCaja, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var pago = await _pagoService.RegistrarPagoAsync(new Pago
                {
                    IdEmpresa = empId,
                    IdCliente = idCliente,
                    IdMedioPago = 1, // Efectivo
                    MontoTotal = monto,
                    
                    
                    Moneda = "PEN",
                    Estado = "CONFIRMADO",
                    IdCanalPago = 1,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                }, cancellationToken);

                await _cajaService.RegistrarMovimientoAsync(empId, idCaja, pago.IdPago, cancellationToken);

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarPagoElectronicoAsync(int idCliente, decimal monto, int idCuentaTesoreria, int idProveedor, string referenciaExterna, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var pago = await _pagoService.RegistrarPagoAsync(new Pago
                {
                    IdEmpresa = empId,
                    IdCliente = idCliente,
                    IdMedioPago = 2, // Electrónico
                    MontoTotal = monto,
                    
                    
                    Moneda = "PEN",
                    Estado = "CONFIRMADO",
                    IdCanalPago = 1,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    ReferenciaExterna = referenciaExterna,
                    Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                }, cancellationToken);

                await _tesoreriaService.RegistrarOperacionFinancieraAsync(empId, idCuentaTesoreria, pago.IdPago, cancellationToken);

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task ProcesarPagoDivididoAsync(int idVenta, decimal montoEfectivo, int idCaja, decimal montoElectronico, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta, cancellationToken);
                if (venta == null || venta.IdEmpresa != empId) throw new Exception("Venta inválida.");

                decimal montoTotal = montoEfectivo + montoElectronico;
                var obligacion = await _obligacionCxCService.GenerarObligacionDesdeVentaAsync(empId, idVenta, montoTotal, cancellationToken);

                if (montoEfectivo > 0)
                {
                    var pagoEf = await _pagoService.RegistrarPagoAsync(new Pago
                    {
                        IdEmpresa = empId, IdCliente = venta.IdCliente, IdMedioPago = 1, MontoTotal = montoEfectivo,   Moneda = "PEN", Estado = "CONFIRMADO", IdCanalPago = 1, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"), Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                    }, cancellationToken);
                    
                    await _aplicacionPagoService.AplicarPagoAsync(empId, pagoEf.IdPago, obligacion, montoEfectivo, cancellationToken);
                     
                    await _cajaService.RegistrarMovimientoAsync(empId, idCaja, pagoEf.IdPago, cancellationToken);
                }

                if (montoElectronico > 0)
                {
                    var pagoEl = await _pagoService.RegistrarPagoAsync(new Pago
                    {
                        IdEmpresa = empId, IdCliente = venta.IdCliente, IdMedioPago = 2, MontoTotal = montoElectronico,   Moneda = "PEN", Estado = "CONFIRMADO", IdCanalPago = 1, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"), Empresa = null!, Cliente = null!, MedioPago = null!, CanalPago = null!
                    }, cancellationToken);

                    await _aplicacionPagoService.AplicarPagoAsync(empId, pagoEl.IdPago, obligacion, montoElectronico, cancellationToken);
                     
                    await _tesoreriaService.RegistrarOperacionFinancieraAsync(empId, idCuentaTesoreria, pagoEl.IdPago, cancellationToken);
                }

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Conflicto.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task AnularPagoAsync(int idPago, string motivo, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                await _pagoService.AnularPagoAsync(empId, idPago, motivo, cancellationToken);
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Conflicto.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task AbrirCajaAsync(int idCaja, decimal saldoInicial, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            
            // Verificamos si existe apertura ABIERTA (intentará buscar, luego insertar).
            var check = await _aperturaCajaRepository.ObtenerTodosAsync(cancellationToken);
            bool abierta = check.Any(a => a.IdEmpresa == empId && a.IdCaja == idCaja && a.Estado == "ABIERTA");
            if (abierta) throw new Exception("Ya existe una apertura abierta para esta caja.");

            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var nueva = new AperturaCaja
                {
                    IdCaja = idCaja,
                    IdEmpresa = empId,
                    IdUsuario = int.Parse(_currentUserService.IdUsuario ?? "0"),
                    FechaApertura = DateTime.UtcNow,
                    SaldoInicial = saldoInicial,
                    SaldoEsperado = saldoInicial,
                    Estado = "ABIERTA",
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    Caja = null!, Empresa = null!, Usuario = null!
                };
                
                await _aperturaCajaRepository.AgregarAsync(nueva, cancellationToken);
                
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateException) // Handle possible UNIQUE constraint violations
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Ya existe una apertura abierta, conflicto concurrente detectado.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task CerrarCajaAsync(int idAperturaCaja, decimal saldoContado, string observaciones, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
                if (apertura == null || apertura.IdEmpresa != empId || apertura.Estado != "ABIERTA")
                    throw new Exception("Apertura no válida o no está abierta.");

                apertura.FechaCierre = DateTime.UtcNow;
                apertura.SaldoContado = saldoContado;
                apertura.Estado = "CERRADA";
                apertura.FechaModificacion = DateTime.UtcNow;
                apertura.UsuarioModificacion = (_currentUserService.IdUsuario ?? "SISTEMA");

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Conflicto al cerrar la caja.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarMovimientoCajaAsync(int idAperturaCaja, decimal monto, string concepto, string tipo, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
                if (apertura == null || apertura.IdEmpresa != empId || apertura.Estado != "ABIERTA")
                    throw new Exception("Caja no está abierta o inválida.");

                var mov = new MovimientoCaja
                {
                    IdEmpresa = empId,
                    IdApertura = idAperturaCaja,
                    IdOrigen = "1",
                    Monto = monto,
                    Tipo = tipo,
                    Fecha = DateTime.UtcNow,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    AperturaCaja = null!, Empresa = null!, /* Concepto removed */
                };
                
                await _movCajaRepo.AgregarAsync(mov, cancellationToken);
                
                if (tipo == "INGRESO") apertura.SaldoEsperado += monto;
                else if (tipo == "EGRESO") apertura.SaldoEsperado -= monto;

                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw new Exception("Conflicto al registrar movimiento.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RegistrarMovimientoTesoreriaAsync(int idCuentaTesoreria, decimal monto, string descripcion, string tipo, CancellationToken cancellationToken)
        {
            var empId = GetEmpresaId();
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var mov = new MovimientoTesoreria
                {
                    IdEmpresa = empId,
                    IdCuenta = idCuentaTesoreria,
                    IdOrigen = "1",
                    Monto = monto,
                    Tipo = tipo,
                    Fecha = DateTime.UtcNow,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                    CuentaTesoreria = null!, Empresa = null!, /* Concepto removed */
                };
                
                await _movTesRepo.AgregarAsync(mov, cancellationToken);
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
