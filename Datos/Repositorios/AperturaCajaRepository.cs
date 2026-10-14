using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class AperturaCajaRepository : RepositorioBase<AperturaCaja>, IAperturaCajaRepository
    {
        public AperturaCajaRepository(ApplicationDbContext contexto) : base(contexto)
        {
        }

        public async Task<AperturaCaja?> ObtenerPorIdAsync(int idEmpresa, int idAperturaCaja, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.IdApertura == idAperturaCaja, cancellationToken);
        }

        public async Task<AperturaCaja?> ObtenerAperturaAbiertaAsync(int idEmpresa, int idCaja, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdEmpresa == idEmpresa && a.IdCaja == idCaja && a.Estado == "ABIERTA", cancellationToken);
        }
    }
}
