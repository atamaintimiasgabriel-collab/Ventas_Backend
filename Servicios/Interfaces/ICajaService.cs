using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface ICajaService
    {
        Task<bool> ValidarCajaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken);
        Task<bool> ValidarEmpresaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken);
        Task<Caja> ObtenerCajaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken);
        Task RegistrarMovimientoAsync(int idEmpresa, int idCaja, int idMovimientoCaja, CancellationToken cancellationToken);
        Task<bool> ValidarAperturaActivaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken);
    }
}
