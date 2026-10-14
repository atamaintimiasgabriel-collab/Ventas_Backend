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
    public class TesoreriaService : ITesoreriaService
    {
        private readonly ICurrentUserService _currentUserService;

        public TesoreriaService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task <CuentaTesoreria> ObtenerCuentaTesoreriaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("La consulta de cuentas de tesorería aún no está implementada.");
        }
        public async Task <bool> ValidarCuentaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task <bool> ValidarEstadoAsync(int idEmpresa, int idCuentaTesoreria, string estado, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task <bool> ValidarEmpresaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task RegistrarOperacionFinancieraAsync(int idEmpresa, int idCuentaTesoreria, int idMovimientoTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            
        }
    }
}
