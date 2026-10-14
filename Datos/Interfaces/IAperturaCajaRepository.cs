using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IAperturaCajaRepository : IRepositorioBase<AperturaCaja>
    {
        Task<AperturaCaja?> ObtenerPorIdAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken = default);
        Task<AperturaCaja?> ObtenerAperturaAbiertaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken = default);
    }
}
