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
    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly ICurrentUserService _currentUserService;

        public PagoService(IPagoRepository pagoRepository, ICurrentUserService currentUserService)
        {
            _pagoRepository = pagoRepository;
            _currentUserService = currentUserService;
        }

        public async Task <Pago> RegistrarPagoAsync(Pago pago, CancellationToken cancellationToken)
        {
            if (pago.MontoTotal <= 0) throw new FinanzasDomainException("El monto del pago debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(pago.Moneda)) pago.Moneda = "PEN";
            if (string.IsNullOrWhiteSpace(pago.Estado)) pago.Estado = "CONFIRMADO";
            pago.FechaCreacion = pago.FechaCreacion == default ? DateTime.Now : pago.FechaCreacion;
            pago.FechaConfirmacion ??= DateTime.Now;
            pago.UsuarioCreacion ??= _currentUserService.Username;

            await _pagoRepository.AgregarAsync(pago, cancellationToken);
            await _pagoRepository.GuardarCambiosAsync(cancellationToken);
            return pago;
        }
        public async Task <Pago> ObtenerPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken)
        {
            var pagos = await _pagoRepository.BuscarAsync(p => p.IdEmpresa == idEmpresa && p.IdPago == idPago, cancellationToken);
            return pagos.FirstOrDefault() ?? throw new FinanzasDomainException("El pago no existe o no pertenece a la empresa actual.");
        }
        public async Task<Pago?> BuscarPorIdempotencyKeyAsync(int idEmpresa, string idempotencyKey, CancellationToken cancellationToken)
        {
            var pagos = await _pagoRepository.BuscarAsync(p => p.IdEmpresa == idEmpresa && p.IdempotencyKey == idempotencyKey, cancellationToken);
            return pagos.FirstOrDefault();
        }
        public async Task<Pago?> BuscarPorReferenciaExternaAsync(int idEmpresa, int idProveedor, string referenciaExterna, CancellationToken cancellationToken)
        {
            var pagos = await _pagoRepository.BuscarAsync(p => p.IdEmpresa == idEmpresa && p.IdProveedor == idProveedor && p.ReferenciaExterna == referenciaExterna, cancellationToken);
            return pagos.FirstOrDefault();
        }
        public async Task ConfirmarPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken)
        {
            var pago = await ObtenerPagoAsync(idEmpresa, idPago, cancellationToken);
            pago.Estado = "CONFIRMADO";
            pago.FechaConfirmacion ??= DateTime.Now;
            pago.UsuarioModificacion = _currentUserService.Username;
            _pagoRepository.Actualizar(pago);
            await _pagoRepository.GuardarCambiosAsync(cancellationToken);
        }
        public async Task AnularPagoAsync(int idEmpresa, int idPago, string motivo, CancellationToken cancellationToken)
        {
            var pago = await ObtenerPagoAsync(idEmpresa, idPago, cancellationToken);
            pago.Estado = "ANULADO";
            pago.UsuarioModificacion = _currentUserService.Username;
            _pagoRepository.Actualizar(pago);
            await _pagoRepository.GuardarCambiosAsync(cancellationToken);
        }
    }
}
