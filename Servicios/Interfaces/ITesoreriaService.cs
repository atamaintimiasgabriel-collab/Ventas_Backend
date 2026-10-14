using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface ITesoreriaService
    {
        Task<CuentaTesoreria> ObtenerCuentaTesoreriaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task<bool> ValidarCuentaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task<bool> ValidarEstadoAsync(int idEmpresa, int idCuentaTesoreria, string estado, CancellationToken cancellationToken);
        Task<bool> ValidarEmpresaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task RegistrarOperacionFinancieraAsync(int idEmpresa, int idCuentaTesoreria, int idMovimientoTesoreria, CancellationToken cancellationToken);
    }
}
