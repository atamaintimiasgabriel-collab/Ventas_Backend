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
    public class AperturaCajaService : IAperturaCajaService
    {
        private readonly IAperturaCajaRepository _aperturaCajaRepository;
        private readonly ICajaRepository _cajaRepository;
        private readonly ICurrentUserService _currentUserService;

        public AperturaCajaService(
            IAperturaCajaRepository aperturaCajaRepository,
            ICajaRepository cajaRepository,
            ICurrentUserService currentUserService)
        {
            _aperturaCajaRepository = aperturaCajaRepository;
            _cajaRepository = cajaRepository;
            _currentUserService = currentUserService;
        }

        public async Task <AperturaCaja> AbrirCajaAsync(int idEmpresa, int idCaja, int idUsuario, decimal saldoInicial, CancellationToken cancellationToken)
        {
            if (saldoInicial < 0) throw new FinanzasDomainException("El saldo inicial no puede ser negativo.");

            var caja = (await _cajaRepository.BuscarAsync(c => c.IdEmpresa == idEmpresa && c.IdCaja == idCaja && c.Activo, cancellationToken)).FirstOrDefault();
            if (caja == null) throw new FinanzasDomainException("La caja no existe, no esta activa o no pertenece a la empresa actual.");

            var activa = (await _aperturaCajaRepository.BuscarAsync(a => a.IdEmpresa == idEmpresa && a.IdCaja == idCaja && a.Estado == "ABIERTA", cancellationToken)).FirstOrDefault();
            if (activa != null) return activa;

            var apertura = new AperturaCaja
            {
                IdEmpresa = idEmpresa,
                IdCaja = idCaja,
                IdUsuario = idUsuario,
                FechaApertura = DateTime.Now,
                SaldoInicial = saldoInicial,
                SaldoEsperado = saldoInicial,
                Estado = "ABIERTA",
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = _currentUserService.Username
            };

            await _aperturaCajaRepository.AgregarAsync(apertura, cancellationToken);
            await _aperturaCajaRepository.GuardarCambiosAsync(cancellationToken);
            return apertura;
        }
        public async Task<AperturaCaja?> ObtenerAperturaActivaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken)
        {
            var aperturas = await _aperturaCajaRepository.BuscarAsync(a => a.IdEmpresa == idEmpresa && a.IdCaja == idCaja && (a.Estado == "ABIERTA" || a.Estado == "PAUSADA"), cancellationToken);
            return aperturas.FirstOrDefault();
        }

        public async Task<AperturaCaja> PausarCajaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            if (apertura == null || apertura.IdEmpresa != idEmpresa) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");
            if (apertura.Estado != "ABIERTA") throw new FinanzasDomainException("Solo una caja abierta puede pausarse.");
            apertura.Estado = "PAUSADA";
            apertura.FechaModificacion = DateTime.Now;
            apertura.UsuarioModificacion = _currentUserService.Username;
            _aperturaCajaRepository.Actualizar(apertura);
            await _aperturaCajaRepository.GuardarCambiosAsync(cancellationToken);
            return apertura;
        }

        public async Task<AperturaCaja> ReanudarCajaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            if (apertura == null || apertura.IdEmpresa != idEmpresa) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");
            if (apertura.Estado != "PAUSADA") throw new FinanzasDomainException("Solo una caja pausada puede reanudarse.");
            apertura.Estado = "ABIERTA";
            apertura.FechaModificacion = DateTime.Now;
            apertura.UsuarioModificacion = _currentUserService.Username;
            _aperturaCajaRepository.Actualizar(apertura);
            await _aperturaCajaRepository.GuardarCambiosAsync(cancellationToken);
            return apertura;
        }
        public async Task <AperturaCaja> CerrarCajaAsync(int idEmpresa, int idAperturaCaja, decimal saldoContado, string observaciones, CancellationToken cancellationToken)
        {
            if (saldoContado < 0) throw new FinanzasDomainException("El saldo contado no puede ser negativo.");
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            if (apertura == null || apertura.IdEmpresa != idEmpresa) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");
            if (apertura.Estado != "ABIERTA") return apertura;

            apertura.Estado = "CERRADA";
            apertura.FechaCierre = DateTime.Now;
            apertura.SaldoContado = saldoContado;
            apertura.UsuarioModificacion = _currentUserService.Username;
            _aperturaCajaRepository.Actualizar(apertura);
            await _aperturaCajaRepository.GuardarCambiosAsync(cancellationToken);
            return apertura;
        }
        public async Task <bool> ValidarAperturaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            return apertura != null && apertura.IdEmpresa == idEmpresa;
        }
        public async Task <decimal> CalcularInformacionCierreAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken)
        {
            var apertura = await _aperturaCajaRepository.ObtenerPorIdAsync(idAperturaCaja, cancellationToken);
            if (apertura == null || apertura.IdEmpresa != idEmpresa) throw new FinanzasDomainException("La apertura no existe o no pertenece a la empresa actual.");
            return apertura.SaldoEsperado;
        }
    }
}
