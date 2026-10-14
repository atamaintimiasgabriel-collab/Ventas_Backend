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
    public class MovimientoCxCService : IMovimientoCxCService
    {
        private readonly ICurrentUserService _currentUserService;

        public MovimientoCxCService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task <MovimientoCxC> RegistrarCargoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de cargos de CxC aún no está implementado.");
        }
        public async Task <MovimientoCxC> RegistrarNotaCreditoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de notas de crédito de CxC aún no está implementado.");
        }
        public async Task <MovimientoCxC> RegistrarAjustePositivoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de ajustes positivos de CxC aún no está implementado.");
        }
        public async Task <MovimientoCxC> RegistrarAjusteNegativoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new NotImplementedException("El registro de ajustes negativos de CxC aún no está implementado.");
        }
    }
}
