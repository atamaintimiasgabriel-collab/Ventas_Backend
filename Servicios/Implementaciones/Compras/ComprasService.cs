using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.DTOs.Compras;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Compras
{
    public class ComprasService : IComprasService
    {
        private readonly ApplicationDbContext _context;

        public ComprasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CompraDTO>> ObtenerComprasAsync(int idEmpresa, int idSucursal)
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(c => c.IdEmpresa == idEmpresa && c.IdSucursal == idSucursal)
                .OrderByDescending(c => c.IdCompra)
                .ToListAsync();

            return compras.Select(MapearACompraDTO);
        }

        public async Task<CompraDTO?> ObtenerCompraPorIdAsync(int idCompra)
        {
            var c = await _context.Compras
                .Include(x => x.Proveedor)
                .Include(x => x.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(x => x.IdCompra == idCompra);

            if (c == null) return null;
            return MapearACompraDTO(c);
        }

        public async Task<CompraDTO> RegistrarCompraAsync(int idEmpresa, int idSucursal, int idUsuario, NuevaCompraDTO request)
        {
            var proveedorValido = await _context.Proveedores
                .AnyAsync(p => p.IdProveedor == request.IdProveedor && p.IdEmpresa == idEmpresa && p.Estado);
            if (!proveedorValido)
                throw new InvalidOperationException("El proveedor no existe, está inactivo o no pertenece a la empresa activa.");

            var idsProductos = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productosValidos = await _context.Productos
                .CountAsync(p => idsProductos.Contains(p.IdProducto) && (p.Estado == null || p.Estado == "Activo" || p.Estado == "ACTIVO"));
            if (productosValidos != idsProductos.Count)
                throw new InvalidOperationException("Uno o más productos no existen, están inactivos o no pertenecen a la empresa activa.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var anio = DateTime.UtcNow.Year;
                var count = await _context.Compras.CountAsync(c => c.IdEmpresa == idEmpresa && c.FechaCompra.Year == anio) + 1;
                var numCompra = $"C-{anio}-{count.ToString("D6")}";

                decimal subtotal = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                decimal impuestoMonto = subtotal * (request.Impuesto / 100m);
                decimal total = subtotal + impuestoMonto;

                var compra = new Compra
                {
                    IdEmpresa = idEmpresa,
                    IdSucursal = idSucursal,
                    IdUsuarioLog = idUsuario,
                    IdProveedor = request.IdProveedor,
                    NumeroCompra = numCompra,
                    FechaCompra = DateTime.UtcNow,
                    FechaEstimada = request.FechaEstimada,
                    Estado = "Pendiente",
                    Subtotal = subtotal,
                    Impuesto = impuestoMonto,
                    Total = total,
                    Observaciones = request.Observaciones,
                    Detalles = request.Detalles.Select(d => new CompraDetalle
                    {
                        IdProducto = d.IdProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerCompraPorIdAsync(compra.IdCompra) ?? throw new Exception("Error al obtener la compra creada");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CompraDTO> CambiarEstadoCompraAsync(int idCompra, CambioEstadoCompraDTO request, int idUsuario)
        {
            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCompra == idCompra);

            if (compra == null)
                throw new Exception("Compra no encontrada.");

            if (compra.Estado == "Anulada")
                throw new Exception("La compra ya se encuentra anulada.");

            if (compra.Estado == request.NuevoEstado)
                return MapearACompraDTO(compra);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (request.NuevoEstado == "Recibida" && compra.Estado != "Recibida")
                {
                    compra.FechaRecepcion = DateTime.UtcNow;
                    await ProcesarEntradaInventarioAsync(compra, idUsuario.ToString(), false);
                }
                
                if (request.NuevoEstado == "Anulada" && compra.Estado == "Recibida")
                {
                    await ProcesarEntradaInventarioAsync(compra, idUsuario.ToString(), true);
                }

                compra.Estado = request.NuevoEstado;
                compra.Observaciones = string.IsNullOrEmpty(request.Observaciones) 
                    ? compra.Observaciones 
                    : compra.Observaciones + "\n" + request.Observaciones;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerCompraPorIdAsync(compra.IdCompra) ?? throw new Exception("Error al retornar compra actualizada.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Dictionary<string, object>> ObtenerKpisAsync(int idEmpresa, int idSucursal)
        {
            var hoy = DateTime.UtcNow;
            var mesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var query = _context.Compras.Where(c => c.IdEmpresa == idEmpresa && c.IdSucursal == idSucursal && c.Estado != "Anulada");
            
            var totalComprasMonto = await query.SumAsync(c => c.Total);
            var totalOrdenes = await query.CountAsync();
            var pendientes = await query.CountAsync(c => c.Estado == "Pendiente");
            
            var comprasMesAnterior = await query.Where(c => c.FechaCompra >= mesActual.AddMonths(-1) && c.FechaCompra < mesActual).SumAsync(c => c.Total);
            var comprasEsteMes = await query.Where(c => c.FechaCompra >= mesActual).SumAsync(c => c.Total);
            var crecimientoMes = comprasMesAnterior == 0 ? 100 : ((comprasEsteMes - comprasMesAnterior) / comprasMesAnterior) * 100;

            var proveedores = await _context.Proveedores.CountAsync(p => p.IdEmpresa == idEmpresa && p.Estado);

            return new Dictionary<string, object>
            {
                { "TotalCompras", totalComprasMonto },
                { "Ordenes", totalOrdenes },
                { "Pendientes", pendientes },
                { "Proveedores", proveedores },
                { "CrecimientoMes", Math.Round(crecimientoMes, 1) },
                { "ComprasEsteMes", comprasEsteMes }
            };
        }

        private async Task ProcesarEntradaInventarioAsync(Compra compra, string usuarioResponsable, bool isReversion)
        {
            var ubicacion = await _context.Ubicaciones
                .Include(u => u.Almacen)
                .Where(u => u.Activo && u.Almacen != null && u.Almacen.IdSucursal == compra.IdSucursal)
                .OrderBy(u => u.IdUbicacion)
                .FirstOrDefaultAsync();
            if (ubicacion == null) throw new Exception("No hay ubicación configurada.");

            var tipoMov = await _context.TiposMovimiento.FirstOrDefaultAsync(t => t.IdTipoMovimiento == 1);
            if (tipoMov == null) throw new Exception("No existe el tipo de movimiento.");

            foreach (var detalle in compra.Detalles)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.IdProducto == detalle.IdProducto && s.IdAlmacen == ubicacion.IdAlmacen && s.IdUbicacion == ubicacion.IdUbicacion);
                if (stock == null)
                {
                    if (isReversion) continue;
                    stock = new Stock 
                    { 
                        IdProducto = detalle.IdProducto, 
                        IdAlmacen = ubicacion.IdAlmacen, 
                        IdUbicacion = ubicacion.IdUbicacion,
                        StockFisico = 0 
                    };
                    _context.Stocks.Add(stock);
                }

                decimal saldoAnterior = stock.StockFisico;
                int cantidadMovimiento = isReversion ? -detalle.Cantidad : detalle.Cantidad;
                decimal saldoActual = saldoAnterior + cantidadMovimiento;

                var mov = new MovimientoInventario
                {
                    IdProducto = detalle.IdProducto,
                    IdAlmacen = ubicacion.IdAlmacen,
                    IdUbicacion = ubicacion.IdUbicacion,
                    IdTipoMovimiento = tipoMov.IdTipoMovimiento,
                    Cantidad = cantidadMovimiento,
                    SaldoAnterior = saldoAnterior,
                    SaldoActual = saldoActual,
                    IdReferenciaOrigen = compra.NumeroCompra,
                    MotivoObservacion = isReversion ? "Reversión por anulación de compra" : "Ingreso por compra recibida",
                    FechaHora = DateTime.UtcNow,
                    UsuarioResponsable = usuarioResponsable
                };

                stock.StockFisico = saldoActual;

                _context.MovimientosInventario.Add(mov);
            }
        }

        private CompraDTO MapearACompraDTO(Compra c)
        {
            return new CompraDTO
            {
                IdCompra = c.IdCompra,
                NumeroCompra = c.NumeroCompra,
                FechaCompra = c.FechaCompra,
                FechaEstimada = c.FechaEstimada,
                FechaRecepcion = c.FechaRecepcion,
                Estado = c.Estado,
                Subtotal = c.Subtotal,
                Impuesto = c.Impuesto,
                Total = c.Total,
                Observaciones = c.Observaciones,
                Proveedor = c.Proveedor == null ? new ProveedorDTO() : new ProveedorDTO
                {
                    IdProveedor = c.Proveedor.IdProveedor,
                    RazonSocial = c.Proveedor.RazonSocial,
                    NombreComercial = c.Proveedor.NombreComercial,
                    NumeroDocumento = c.Proveedor.NumeroDocumento
                },
                Detalles = c.Detalles.Select(d => new CompraDetalleDTO
                {
                    IdCompraDetalle = d.IdCompraDetalle,
                    IdProducto = d.IdProducto,
                    NombreProducto = d.Producto?.NombreComercial ?? "Desconocido",
                    CodigoProducto = d.Producto?.SKU ?? "",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }
    }
}
