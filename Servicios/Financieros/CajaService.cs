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
    public class CajaService : ICajaService
    {
        private readonly ICajaRepository _cajaRepository;
        private readonly IAperturaCajaRepository _aperturaCajaRepository;
        private readonly ICurrentUserService _currentUserService;

        public CajaService(
            ICajaRepository cajaRepository,
            IAperturaCajaRepository aperturaCajaRepository,
            ICurrentUserService currentUserService)
        {
            _cajaRepository = cajaRepository;
            _aperturaCajaRepository = aperturaCajaRepository;
            _currentUserService = currentUserService;
        }

        public async Task <bool> ValidarCajaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken)
        {
            var caja = await ObtenerCajaAsync(idEmpresa, idCaja, cancellationToken);
            return caja != null && caja.Activo;
        }
        public async Task <bool> ValidarEmpresaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken)
        {
            var caja = await _cajaRepository.ObtenerPorIdAsync(idCaja, cancellationToken);
            return caja != null && caja.IdEmpresa == idEmpresa;
        }
        public async Task <Caja> ObtenerCajaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken)
        {
            var cajas = await _cajaRepository.BuscarAsync(c => c.IdEmpresa == idEmpresa && c.IdCaja == idCaja, cancellationToken);
            return cajas.FirstOrDefault() ?? throw new FinanzasDomainException("La caja no existe o no pertenece a la empresa actual.");
        }
        public async Task RegistrarMovimientoAsync(int idEmpresa, int idCaja, int idMovimientoCaja, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            
        }
        public async Task <bool> ValidarAperturaActivaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken)
        {
            var aperturas = await _aperturaCajaRepository.BuscarAsync(
                a => a.IdEmpresa == idEmpresa && a.IdCaja == idCaja && a.Estado == "ABIERTA",
                cancellationToken);
            return aperturas.Any();
        }
    }
}
