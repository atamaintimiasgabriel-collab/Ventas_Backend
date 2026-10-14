using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1;

/// <summary>Consultas de lectura para el panel de reportes. No persiste archivos ni altera operaciones.</summary>
[ApiController]
[Route("api/v1/reportes")]
[Authorize(Policy = "REPORTES_CONSULTAR")]
public sealed class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ReportesController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string? tipo, CancellationToken cancellationToken)
    {
        var empresa = EmpresaActual();
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var anteriorInicio = inicio.AddDays(-(fin - inicio).TotalDays - 1);
        var anteriorFin = inicio.AddTicks(-1);

        var ventas = _db.Ventas.AsNoTracking().Where(v => v.IdEmpresa == empresa && v.FechaVenta >= inicio && v.FechaVenta <= fin && v.Estado != "ANULADA");
        var compras = _db.Compras.AsNoTracking().Where(c => c.IdEmpresa == empresa && c.FechaCompra >= inicio && c.FechaCompra <= fin && c.Estado != "Anulada");
        var ventaDetalles = _db.VentaDetalles.AsNoTracking().Where(d => d.Venta.IdEmpresa == empresa && d.Venta.FechaVenta >= inicio && d.Venta.FechaVenta <= fin && d.Venta.Estado != "ANULADA");

        var totalVentas = await ventas.SumAsync(v => (decimal?)v.Total, cancellationToken) ?? 0;
        var totalCompras = await compras.SumAsync(c => (decimal?)c.Total, cancellationToken) ?? 0;
        var productosVendidos = await ventaDetalles.SumAsync(d => (decimal?)d.Cantidad, cancellationToken) ?? 0;
        var clientesAtendidos = await ventas.Select(v => v.IdCliente).Distinct().CountAsync(cancellationToken);
        var costos = await ventaDetalles.SumAsync(d => (decimal?)(d.Cantidad * d.CostoUnitario), cancellationToken) ?? 0;

        var ventasPrevias = _db.Ventas.AsNoTracking().Where(v => v.IdEmpresa == empresa && v.FechaVenta >= anteriorInicio && v.FechaVenta <= anteriorFin && v.Estado != "ANULADA");
        var comprasPrevias = _db.Compras.AsNoTracking().Where(c => c.IdEmpresa == empresa && c.FechaCompra >= anteriorInicio && c.FechaCompra <= anteriorFin && c.Estado != "Anulada");
        var detallesPrevios = _db.VentaDetalles.AsNoTracking().Where(d => d.Venta.IdEmpresa == empresa && d.Venta.FechaVenta >= anteriorInicio && d.Venta.FechaVenta <= anteriorFin && d.Venta.Estado != "ANULADA");
        var totalVentasPrevias = await ventasPrevias.SumAsync(v => (decimal?)v.Total, cancellationToken) ?? 0;
        var totalComprasPrevias = await comprasPrevias.SumAsync(c => (decimal?)c.Total, cancellationToken) ?? 0;
        var productosPrevios = await detallesPrevios.SumAsync(d => (decimal?)d.Cantidad, cancellationToken) ?? 0;
        var clientesPrevios = await ventasPrevias.Select(v => v.IdCliente).Distinct().CountAsync(cancellationToken);
        var costosPrevios = await detallesPrevios.SumAsync(d => (decimal?)(d.Cantidad * d.CostoUnitario), cancellationToken) ?? 0;

        var ventasDiarias = await ventas.GroupBy(v => v.FechaVenta.Date).Select(g => new { Fecha = g.Key, Ventas = g.Sum(x => x.Total) }).ToListAsync(cancellationToken);
        var comprasDiarias = await compras.GroupBy(c => c.FechaCompra.Date).Select(g => new { Fecha = g.Key, Compras = g.Sum(x => x.Total) }).ToListAsync(cancellationToken);
        var utilidadDiaria = await ventaDetalles.GroupBy(d => d.Venta.FechaVenta.Date).Select(g => new { Fecha = g.Key, Utilidad = g.Sum(x => x.TotalLinea - (x.Cantidad * x.CostoUnitario)) }).ToListAsync(cancellationToken);
        var ventasPorDia = await ventas.GroupBy(v => v.FechaVenta.Date).Select(g => new { Fecha = g.Key, Ventas = g.Count(), Clientes = g.Select(x => x.IdCliente).Distinct().Count(), Total = g.Sum(x => x.Total) }).ToListAsync(cancellationToken);
        var productosPorDia = await ventaDetalles.GroupBy(d => d.Venta.FechaVenta.Date).Select(g => new { Fecha = g.Key, Productos = g.Sum(x => x.Cantidad), Utilidad = g.Sum(x => x.TotalLinea - (x.Cantidad * x.CostoUnitario)) }).ToListAsync(cancellationToken);

        var categorias = await ventaDetalles.GroupBy(d => d.Producto.Categoria.Nombre).Select(g => new { Nombre = g.Key, Cantidad = g.Sum(x => x.Cantidad) }).OrderByDescending(x => x.Cantidad).ToListAsync(cancellationToken);
        var topProductos = await ventaDetalles.GroupBy(d => d.Producto.NombreComercial).Select(g => new { Nombre = g.Key, Cantidad = g.Sum(x => x.Cantidad) }).OrderByDescending(x => x.Cantidad).Take(5).ToListAsync(cancellationToken);
        var marcas = await ventaDetalles.GroupBy(d => d.Producto.Marca.Nombre).Select(g => new { Nombre = g.Key, Cantidad = g.Sum(x => x.Cantidad) }).OrderByDescending(x => x.Cantidad).Take(5).ToListAsync(cancellationToken);

        var totalCategoria = categorias.Sum(x => x.Cantidad);
        var totalMarca = marcas.Sum(x => x.Cantidad);
        var productoMaximo = topProductos.FirstOrDefault()?.Cantidad ?? 0;
        var fechas = ventasDiarias.Select(x => x.Fecha).Concat(comprasDiarias.Select(x => x.Fecha)).Distinct().OrderBy(x => x).ToList();
        var filas = ventasPorDia.Join(productosPorDia, v => v.Fecha, p => p.Fecha, (v, p) => new { v.Fecha, v.Ventas, p.Productos, v.Clientes, v.Total, p.Utilidad })
            .OrderByDescending(x => x.Fecha).Take(100).ToList();

        return Ok(new
        {
            periodo = new { fechaInicio = inicio, fechaFin = fin },
            kpis = new[]
            {
                Kpi("Ventas Totales", totalVentas, Crecimiento(totalVentas, totalVentasPrevias), "PEN"),
                Kpi("Productos Vendidos", productosVendidos, Crecimiento(productosVendidos, productosPrevios), null),
                Kpi("Clientes Atendidos", clientesAtendidos, Crecimiento(clientesAtendidos, clientesPrevios), null),
                Kpi("Compras Totales", totalCompras, Crecimiento(totalCompras, totalComprasPrevias), "PEN"),
                Kpi("Utilidad Estimada", totalVentas - costos, Crecimiento(totalVentas - costos, totalVentasPrevias - costosPrevios), "PEN")
            },
            ventasCompras = fechas.Select(f => new { fecha = f, ventas = ventasDiarias.FirstOrDefault(x => x.Fecha == f)?.Ventas ?? 0, compras = comprasDiarias.FirstOrDefault(x => x.Fecha == f)?.Compras ?? 0, utilidad = utilidadDiaria.FirstOrDefault(x => x.Fecha == f)?.Utilidad ?? 0 }),
            categorias = categorias.Select(x => new { nombre = x.Nombre, cantidad = x.Cantidad, porcentaje = totalCategoria == 0 ? 0 : Math.Round(x.Cantidad * 100m / totalCategoria, 2) }),
            topProductos = topProductos.Select(x => new { nombre = x.Nombre, cantidad = x.Cantidad, porcentaje = productoMaximo == 0 ? 0 : Math.Round(x.Cantidad * 100m / productoMaximo, 2) }),
            marcas = marcas.Select(x => new { nombre = x.Nombre, cantidad = x.Cantidad, porcentaje = totalMarca == 0 ? 0 : Math.Round(x.Cantidad * 100m / totalMarca, 2) }),
            resumenVentas = filas,
            reportesGenerados = Array.Empty<object>(),
            tipo = tipo ?? "Ventas"
        });
    }

    [HttpPost("exportar")]
    public async Task<IActionResult> Exportar([FromBody] ExportarReporteRequest request, CancellationToken cancellationToken)
    {
        var empresa = EmpresaActual();
        var (inicio, fin) = Rango(request.FechaInicio, request.FechaFin);
        var filas = await _db.Ventas.AsNoTracking()
            .Where(v => v.IdEmpresa == empresa && v.FechaVenta >= inicio && v.FechaVenta <= fin && v.Estado != "ANULADA")
            .OrderBy(v => v.FechaVenta)
            .Select(v => new { v.FechaVenta, v.NumeroVenta, Cliente = v.Cliente.RazonSocial ?? (v.Cliente.Nombres + " " + v.Cliente.Apellidos), v.Total, v.Estado })
            .ToListAsync(cancellationToken);
        var csv = new StringBuilder("Fecha;Numero venta;Cliente;Total;Estado\r\n");
        foreach (var fila in filas) csv.AppendLine($"{fila.FechaVenta:yyyy-MM-dd};{Csv(fila.NumeroVenta)};{Csv(fila.Cliente)};{fila.Total.ToString("0.00", CultureInfo.InvariantCulture)};{Csv(fila.Estado)}");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", $"reporte-ventas-{inicio:yyyyMMdd}-{fin:yyyyMMdd}.csv");
    }

    private int EmpresaActual() => _currentUser.IdEmpresa is > 0 ? _currentUser.IdEmpresa.Value : throw new UnauthorizedAccessException("El usuario no tiene empresa asignada.");
    private static (DateTime Inicio, DateTime Fin) Rango(DateTime? desde, DateTime? hasta)
    {
        var fin = (hasta?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);
        var inicio = desde?.Date ?? fin.Date.AddDays(-29);
        if (inicio > fin || inicio < fin.AddYears(-2)) throw new ArgumentException("El rango de fechas no es valido o supera dos anos.");
        return (inicio, fin);
    }
    private static object Kpi(string etiqueta, decimal valor, decimal crecimiento, string? moneda) => new { etiqueta, valor, crecimiento, moneda };
    private static decimal Crecimiento(decimal actual, decimal anterior) => anterior == 0 ? (actual == 0 ? 0 : 100) : Math.Round((actual - anterior) * 100m / Math.Abs(anterior), 2);
    private static string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}

public sealed class ExportarReporteRequest
{
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
}
