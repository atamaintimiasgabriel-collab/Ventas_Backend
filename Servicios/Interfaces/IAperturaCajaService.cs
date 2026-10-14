using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IAperturaCajaService
    {
        Task<AperturaCaja> AbrirCajaAsync(int idEmpresa, int idCaja, int idUsuario, decimal saldoInicial, CancellationToken cancellationToken);
        Task<AperturaCaja?> ObtenerAperturaActivaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken);
        Task<AperturaCaja> CerrarCajaAsync(int idEmpresa, int idAperturaCaja, decimal saldoContado, string observaciones, CancellationToken cancellationToken);
        Task<bool> ValidarAperturaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<decimal> CalcularInformacionCierreAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<AperturaCaja> PausarCajaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<AperturaCaja> ReanudarCajaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
    }
}
