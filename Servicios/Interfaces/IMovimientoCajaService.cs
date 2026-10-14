using System;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IMovimientoCajaService
    {
        Task<MovimientoCaja> RegistrarIngresoAsync(int idEmpresa, int idAperturaCaja, decimal monto, string concepto, CancellationToken cancellationToken);
        Task<MovimientoCaja> RegistrarEgresoAsync(int idEmpresa, int idAperturaCaja, decimal monto, string concepto, CancellationToken cancellationToken);
        Task<bool> ValidarAperturaExistenteAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<bool> ValidarAperturaAbiertaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<bool> ValidarEmpresaAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken);
        Task<bool> ValidarMontoPositivoAsync(decimal monto, CancellationToken cancellationToken);
    }
}
