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
    public class MovimientoTesoreriaService : IMovimientoTesoreriaService
    {
        private readonly ICurrentUserService _currentUserService;

        public MovimientoTesoreriaService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task <MovimientoTesoreria> RegistrarIngresoAsync(int idEmpresa, int idCuentaTesoreria, decimal monto, string descripcion, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de ingresos de tesorería aún no está implementado.");
        }
        public async Task <MovimientoTesoreria> RegistrarEgresoAsync(int idEmpresa, int idCuentaTesoreria, decimal monto, string descripcion, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de egresos de tesorería aún no está implementado.");
        }
        public async Task <bool> ValidarCuentaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task <bool> ValidarEmpresaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task <bool> ValidarMontoAsync(decimal monto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
        public async Task <bool> ValidarEstadoAsync(int idEmpresa, int idMovimientoTesoreria, string estado, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }
    }
}
