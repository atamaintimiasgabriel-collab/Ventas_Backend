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
    public class MovimientoCajaService : IMovimientoCajaService
    {
        private readonly IMovimientoCajaRepository _movimientoCajaRepository;
        private readonly IAperturaCajaRepository _aperturaCajaRepository;
        private readonly ICurrentUserService _currentUserService;

        public MovimientoCajaService(
            IMovimientoCajaRepository movimientoCajaRepository,
            IAperturaCajaRepository aperturaCajaRepository,
            ICurrentUserService currentUserService)
        {
            _movimientoCajaRepository = movimientoCajaRepository;
            _aperturaCajaRepository = aperturaCajaRepository;
            _currentUserService = currentUserService;
        }

        public async Task <MovimientoCaja> RegistrarIngresoAsync(int idEmpresa, int idAperturaCaja, decimal monto, string concepto, CancellationToken cancellationToken)
        {
            return await RegistrarMovimientoAsync(idEmpresa, idAperturaCaja, "INGRESO", monto, concepto, cancellationToken);
        }
        public async Task <MovimientoCaja> RegistrarEgresoAsync(int idEmpresa, int idAperturaCaja, decimal monto, string concepto, CancellationToken cancellationToken)
        {
            return await RegistrarMovimientoAsync(idEmpresa, idAperturaCaja, "EGRESO", monto, concepto, cancellationToken);
        }
        public async Task <bool> ValidarAperturaExistenteAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            return apertura != null && apertura.IdEmpresa == idEmpresa;
        }
        public async Task <bool> ValidarAperturaAbiertaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            return apertura != null && apertura.IdEmpresa == idEmpresa && apertura.Estado == "ABIERTA";
        }
        public async Task <bool> ValidarEmpresaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            return await ValidarAperturaExistenteAsync(idEmpresa, idAperturaCaja, cancellationToken);
        }
        public async Task <bool> ValidarMontoPositivoAsync(decimal monto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return monto > 0;
        }

        private async Task<MovimientoCaja> RegistrarMovimientoAsync(int idEmpresa, int idAperturaCaja, string tipo, decimal monto, string concepto, CancellationToken cancellationToken)
        {
            if (monto <= 0) throw new FinanzasDomainException("El monto debe ser mayor a cero.");
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            if (apertura == null || apertura.IdEmpresa != idEmpresa) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");
            if (apertura.Estado != "ABIERTA") throw new FinanzasDomainException("La apertura de caja no esta abierta.");

            var movimiento = new MovimientoCaja
            {
                IdEmpresa = idEmpresa,
                IdApertura = idAperturaCaja,
                Tipo = tipo,
                Monto = monto,
                IdOrigen = concepto,
                Fecha = DateTime.Now,
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = _currentUserService.Username
            };

            apertura.SaldoEsperado += tipo == "INGRESO" ? monto : -monto;
            _aperturaCajaRepository.Actualizar(apertura);
            await _movimientoCajaRepository.AgregarAsync(movimiento, cancellationToken);
            await _movimientoCajaRepository.GuardarCambiosAsync(cancellationToken);
            return movimiento;
        }
    }
}
