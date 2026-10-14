using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class VentaRepository : RepositorioBase<Venta>, IVentaRepository
    {
        public VentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Venta?> ObtenerVentaPorIdAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta && v.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<Venta?> ObtenerVentaConDetallesAsync(int idEmpresa, int idVenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.VentaDetalles)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta && v.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<Venta?> ObtenerVentaPorNumeroAsync(int idEmpresa, string numeroVenta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.NumeroVenta == numeroVenta && v.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<Venta?> ObtenerVentaPorPedidoAsync(int idEmpresa, int idPedido, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdPedido == idPedido && v.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<bool> ExisteVentaParaPedidoAsync(int idEmpresa, int idPedido, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(v => v.IdPedido == idPedido && v.IdEmpresa == idEmpresa, cancellationToken);
        }

        public async Task<(IEnumerable<Venta> Ventas, int TotalRecords)> ObtenerVentasPaginadasAsync(
            int idEmpresa, 
            int pageNumber, 
            int pageSize,
            string? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            int? idCliente = null,
            string? origen = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(v => v.IdEmpresa == idEmpresa);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(v => v.Estado == estado);

            if (fechaDesde.HasValue)
                query = query.Where(v => v.FechaVenta >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.FechaVenta <= fechaHasta.Value);

            if (idSucursal.HasValue)
                query = query.Where(v => v.IdSucursal == idSucursal.Value);

            if (idCliente.HasValue)
                query = query.Where(v => v.IdCliente == idCliente.Value);

            if (origen != null)
                query = query.Where(v => v.Origen == origen);

            int totalRecords = await query.CountAsync(cancellationToken);

            var ventas = await query
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.IdVenta)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (ventas, totalRecords);
        }
    }
}
