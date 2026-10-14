using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IAplicacionPagoService
    {
        Task<AplicacionPago> AplicarPagoAsync(int idEmpresa, int idPago, ObligacionCxC obligacion, decimal montoAplicado, CancellationToken cancellationToken);
        Task<AplicacionPago> AplicarParcialmenteAsync(int idEmpresa, int idPago, int idObligacion, decimal montoAplicado, CancellationToken cancellationToken);
        Task<IEnumerable<AplicacionPago>> AplicarAMultiplesObligacionesAsync(int idEmpresa, int idPago, IEnumerable<int> idObligaciones, IEnumerable<decimal> montos, CancellationToken cancellationToken);
        Task<IEnumerable<AplicacionPago>> ObtenerAplicacionesPorPagoAsync(int idEmpresa, int idPago, CancellationToken cancellationToken);
        Task<IEnumerable<AplicacionPago>> ObtenerAplicacionesPorObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken);
        Task<bool> ValidarMontoDisponibleAsync(int idEmpresa, int idPago, decimal montoRequerido, CancellationToken cancellationToken);
        Task<bool> ValidarSaldoDisponibleAsync(int idEmpresa, int idObligacion, decimal montoRequerido, CancellationToken cancellationToken);
    }
}

