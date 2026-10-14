using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Inventario;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Inventario;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class InventarioService : IInventarioService
    {
        private readonly IRepositorioBase<Stock> _stockRepository;
        private readonly IRepositorioBase<MovimientoInventario> _movimientoRepository;
        private readonly IRepositorioBase<TipoMovimiento> _tipoMovimientoRepository;
        private readonly IRepositorioBase<Almacen> _almacenRepository;

        public InventarioService(
            IRepositorioBase<Stock> stockRepository,
            IRepositorioBase<MovimientoInventario> movimientoRepository,
            IRepositorioBase<TipoMovimiento> tipoMovimientoRepository,
            IRepositorioBase<Almacen> almacenRepository)
        {
            _stockRepository = stockRepository;
            _movimientoRepository = movimientoRepository;
            _tipoMovimientoRepository = tipoMovimientoRepository;
            _almacenRepository = almacenRepository;
        }

        public async Task DescontarStockPorVentaAsync(int idEmpresa, int idVenta, string origen, IEnumerable<DescuentoStockRequest> items, CancellationToken cancellationToken = default)
        {
            if (items == null || !items.Any())
                return;

            string idReferencia = $"{origen}-{idVenta}";

            // Verificar idempotencia (doble procesamiento bloqueado)
            var movimientos = await _movimientoRepository.BuscarAsync(m => m.IdReferenciaOrigen == idReferencia, cancellationToken);
            
            if (movimientos.Any())
            {
                return; // Ya procesado, idempotencia
            }

            // Obtener tipo de movimiento VEN_SAL
            var tipos = await _tipoMovimientoRepository.BuscarAsync(t => t.Codigo == "VEN_SAL", cancellationToken);
            var tipoMovimiento = tipos.FirstOrDefault();
            
            if (tipoMovimiento == null)
            {
                throw new InventarioDomainException("El tipo de movimiento VEN_SAL no está configurado en el sistema.");
            }

            foreach (var item in items)
            {
                if (item.Cantidad <= 0)
                    throw new InventarioDomainException($"La cantidad a descontar para el producto {item.IdProducto} debe ser mayor a cero.");

                // Validar Almacén (Multiempresa PASS, confiamos en IdEmpresa validado en controller/venta service)
                var almacen = await _almacenRepository.ObtenerPorIdAsync(item.IdAlmacen, cancellationToken);
                if (almacen == null)
                {
                    throw new InventarioDomainException($"El almacén {item.IdAlmacen} no existe.");
                }

                Stock? stock = null;
                
                if (item.IdUbicacion > 0)
                {
                    var stocks = await _stockRepository.BuscarAsync(s => s.IdProducto == item.IdProducto && s.IdAlmacen == item.IdAlmacen && s.IdUbicacion == item.IdUbicacion, cancellationToken);
                    stock = stocks.FirstOrDefault();
                }
                else
                {
                    // Buscar cualquier ubicación con stock suficiente en este almacén
                    var stocks = await _stockRepository.BuscarAsync(s => s.IdProducto == item.IdProducto && s.IdAlmacen == item.IdAlmacen, cancellationToken);
                    stock = stocks.OrderByDescending(s => s.StockFisico - s.StockReservado - s.StockSeguridad).FirstOrDefault();
                }

                if (stock == null)
                {
                    throw new InventarioDomainException($"No se encontró stock para el producto {item.IdProducto} en el almacén {item.IdAlmacen}.");
                }

                // Validar disponibilidad
                decimal stockDisponible = stock.StockFisico - stock.StockReservado - stock.StockSeguridad;
                if (stockDisponible < item.Cantidad)
                {
                    throw new InventarioDomainException($"Stock insuficiente para el producto {item.IdProducto}. Disponible: {stockDisponible}, Solicitado: {item.Cantidad}.");
                }

                // Descontar Stock Físico (el Disponible es una computed column o se actualiza)
                decimal saldoAnterior = stock.StockFisico;
                stock.StockFisico -= item.Cantidad;
                stock.FechaUltimaActualizacion = DateTime.UtcNow;
                stock.UsuarioUltimaActualizacion = "SISTEMA"; // ideal: CurrentUser, pero validaremos en middleware

                _stockRepository.Actualizar(stock);

                // Registrar Movimiento
                var movimiento = new MovimientoInventario
                {
                    IdProducto = item.IdProducto,
                    IdAlmacen = item.IdAlmacen,
                    IdUbicacion = stock.IdUbicacion,
                    IdTipoMovimiento = tipoMovimiento.IdTipoMovimiento,
                    Cantidad = item.Cantidad,
                    SaldoAnterior = saldoAnterior,
                    SaldoActual = stock.StockFisico,
                    FechaHora = DateTime.UtcNow,
                    UsuarioResponsable = "SISTEMA", // ideal: CurrentUser
                    MotivoObservacion = $"Descuento por {origen} Nro {idVenta}",
                    IdReferenciaOrigen = idReferencia
                };

                await _movimientoRepository.AgregarAsync(movimiento, cancellationToken);
            }
        }
    }
}
