using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IMovimientoTesoreriaService
    {
        Task<MovimientoTesoreria> RegistrarIngresoAsync(int idEmpresa, int idCuentaTesoreria, decimal monto, string descripcion, CancellationToken cancellationToken);
        Task<MovimientoTesoreria> RegistrarEgresoAsync(int idEmpresa, int idCuentaTesoreria, decimal monto, string descripcion, CancellationToken cancellationToken);
        Task<bool> ValidarCuentaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task<bool> ValidarEmpresaAsync(int idEmpresa, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task<bool> ValidarMontoAsync(decimal monto, CancellationToken cancellationToken);
        Task<bool> ValidarEstadoAsync(int idEmpresa, int idMovimientoTesoreria, string estado, CancellationToken cancellationToken);
    }
}
