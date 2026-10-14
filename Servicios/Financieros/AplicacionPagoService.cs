using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Financieros
{
    public class AplicacionPagoService : IAplicacionPagoService
    {
        private readonly IAplicacionPagoRepository _aplicacionPagoRepository;
        private readonly IPagoRepository _pagoRepository;
        private readonly IObligacionCxCRepository _obligacionRepository;
        private readonly ICurrentUserService _currentUserService;

        public AplicacionPagoService(
            IAplicacionPagoRepository aplicacionPagoRepository,
            IPagoRepository pagoRepository,
            IObligacionCxCRepository obligacionRepository,
            ICurrentUserService currentUserService)
        {
            _aplicacionPagoRepository = aplicacionPagoRepository;
            _pagoRepository = pagoRepository;
            _obligacionRepository = obligacionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<AplicacionPago> AplicarPagoAsync(int idEmpresa, int idPago, ObligacionCxC obligacion, decimal montoAplicado, CancellationToken cancellationToken)
        {
            if (montoAplicado <= 0) throw new FinanzasDomainException("El monto aplicado debe ser mayor a cero.");
            var pago = (await _pagoRepository.BuscarAsync(p => p.IdEmpresa == idEmpresa && p.IdPago == idPago, cancellationToken)).FirstOrDefault();
            if (pago == null) throw new FinanzasDomainException("El pago no existe o no pertenece a la empresa actual.");
            if (obligacion == null) throw new FinanzasDomainException("La obligacion no existe o no pertenece a la empresa actual.");

            var existente = (await _aplicacionPagoRepository.BuscarAsync(a => a.IdEmpresa == idEmpresa && a.IdPago == idPago && a.IdObligacion == obligacion.IdObligacion, cancellationToken)).FirstOrDefault();
            if (existente != null) return existente;

            var totalAplicadoPrevio = await _aplicacionPagoRepository.ObtenerMontoAplicadoPorObligacionAsync(idEmpresa, obligacion.IdObligacion, cancellationToken);
            var saldoPendiente = obligacion.DeudaOriginal - totalAplicadoPrevio;
            if (saldoPendiente <= 0) throw new FinanzasDomainException("La obligacion ya no tiene saldo pendiente.");
            if (montoAplicado > saldoPendiente) throw new FinanzasDomainException("El monto aplicado excede el saldo pendiente de la obligacion.");

            var aplicacion = new AplicacionPago
            {
                IdEmpresa = idEmpresa,
                IdPago = idPago,
                IdObligacion = obligacion.IdObligacion,
                MontoAplicado = montoAplicado,
                Fecha = DateTime.Now,
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = _currentUserService.Username
            };

            var totalAplicado = totalAplicadoPrevio + montoAplicado;
            obligacion.Estado = totalAplicado >= obligacion.DeudaOriginal ? "PAGADA" : "PARCIAL";
            obligacion.FechaModificacion = DateTime.Now;
            obligacion.UsuarioModificacion = _currentUserService.Username;
            await _aplicacionPagoRepository.AgregarAsync(aplicacion, cancellationToken);
            await _aplicacionPagoRepository.GuardarCambiosAsync(cancellationToken);
            return aplicacion;
        }
        public async Task<AplicacionPago> AplicarParcialmenteAsync(int idEmpresa, int idPago, int idObligacion, decimal montoAplicado, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("La aplicación parcial de pagos aún no está implementada.");
        }
        public async Task<IEnumerable<AplicacionPago>> AplicarAMultiplesObligacionesAsync(int idEmpresa, int idPago, IEnumerable<int> idObligaciones, IEnumerable<decimal> montos, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("La aplicación de pagos a múltiples obligaciones aún no está implementada.");
        }
        public async Task<IEnumerable<AplicacionPago>> ObtenerAplicacionesPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("La consulta de aplicaciones por pago aún no está implementada.");
        }
        public async Task<IEnumerable<AplicacionPago>> ObtenerAplicacionesPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("La consulta de aplicaciones por obligación aún no está implementada.");
        }
        public async Task<bool> ValidarMontoDisponibleAsync(int idEmpresa, int idPago, decimal montoRequerido, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return false;
        }
        public async Task<bool> ValidarSaldoDisponibleAsync(int idEmpresa, int idObligacion, decimal montoRequerido, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return false;
        }
    }
}
