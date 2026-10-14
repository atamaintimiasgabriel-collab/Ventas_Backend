using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IObligacionCxCService
    {
        Task<ObligacionCxC> RegistrarObligacionAsync(ObligacionCxC obligacion, CancellationToken cancellationToken);
        Task<ObligacionCxC> ObtenerObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken);
        Task<decimal> ConsultarSaldoAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken);
        Task<bool> ValidarEstadoAsync(int idEmpresa, int idObligacion, string estado, CancellationToken cancellationToken);
        Task<bool> ValidarEmpresaAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken);
        Task<bool> ValidarVencimientoAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken);
        Task<ObligacionCxC> GenerarObligacionDesdeVentaAsync(int idEmpresa, int idVenta, decimal montoTotal, CancellationToken cancellationToken);
    }
}
