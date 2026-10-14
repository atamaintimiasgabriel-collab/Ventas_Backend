using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IMovimientoCxCService
    {
        Task<MovimientoCxC> RegistrarCargoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken);
        Task<MovimientoCxC> RegistrarNotaCreditoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken);
        Task<MovimientoCxC> RegistrarAjustePositivoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken);
        Task<MovimientoCxC> RegistrarAjusteNegativoAsync(int idEmpresa, int idObligacion, decimal monto, string motivo, CancellationToken cancellationToken);
    }
}
