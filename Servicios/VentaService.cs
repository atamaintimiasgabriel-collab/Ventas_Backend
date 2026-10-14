using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Inventario;
using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;
using SistemaVentasMotorrepuestos.API.DTOs.Ventas;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IRepositorioBase<Producto> _productoRepository;
        private readonly IRepositorioBase<UnidadMedida> _unidadMedidaRepository;
        private readonly IRepositorioBase<Cliente> _clienteRepository;
        private readonly IRepositorioBase<Sucursal> _sucursalRepository;
        private readonly IRepositorioBase<Almacen> _almacenRepository;
        private readonly IRepositorioBase<Usuario> _usuarioRepository;
        private readonly IUnidadDeTrabajo _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly INumeradorVenta _numerador;
        private readonly IInventarioService _inventarioService;
        private readonly ITaxEngine _taxEngine;

        public VentaService(
            IVentaRepository ventaRepository,
            IPedidoRepository pedidoRepository,
            IRepositorioBase<Producto> productoRepository,
            IRepositorioBase<UnidadMedida> unidadMedidaRepository,
            IRepositorioBase<Cliente> clienteRepository,
            IRepositorioBase<Sucursal> sucursalRepository,
            IRepositorioBase<Almacen> almacenRepository,
            IRepositorioBase<Usuario> usuarioRepository,
            IUnidadDeTrabajo uow,
            ICurrentUserService currentUser,
            INumeradorVenta numerador,
            IInventarioService inventarioService,
            ITaxEngine taxEngine)
        {
            _ventaRepository = ventaRepository;
            _pedidoRepository = pedidoRepository;
            _productoRepository = productoRepository;
            _unidadMedidaRepository = unidadMedidaRepository;
            _clienteRepository = clienteRepository;
            _sucursalRepository = sucursalRepository;
            _almacenRepository = almacenRepository;
            _usuarioRepository = usuarioRepository;
            _uow = uow;
            _currentUser = currentUser;
            _numerador = numerador;
            _inventarioService = inventarioService;
            _taxEngine = taxEngine;
        }

        private int ObtenerEmpresaActual()
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                throw new VentaDomainException("El usuario autenticado no tiene una empresa asignada o el token es inválido.");
            return idEmpresa.Value;
        }

        private async Task<int> ObtenerUsuarioVendedorActualAsync(CancellationToken cancellationToken)
        {
            var idUsuarioStr = _currentUser.IdUsuario;
            if (string.IsNullOrWhiteSpace(idUsuarioStr))
                throw new VentaDomainException("El token no contiene un identificador de usuario válido.");
            
            if (!int.TryParse(idUsuarioStr, out var idUsuario))
                throw new VentaDomainException("El identificador de usuario en el token no es numérico.");

            var usuarios = await _usuarioRepository.BuscarAsync(u => u.IdUsuario == idUsuario, cancellationToken);
            var usuario = usuarios.FirstOrDefault();

            if (usuario == null)
                throw new VentaDomainException("El usuario vendedor no existe en el sistema.");
            
            if (!usuario.Activo)
                throw new VentaDomainException("El usuario vendedor se encuentra inactivo.");

            return usuario.IdUsuario;
        }

        public async Task<VentaResponse> CrearVentaAsync(CrearVentaRequest request, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var idUsuarioVendedor = await ObtenerUsuarioVendedorActualAsync(cancellationToken);

            if (request.Productos == null || request.Productos.Count == 0)
                throw new VentaDomainException("La venta debe contener al menos un producto.");
            if (request.DescuentoGlobal < 0)
                throw new VentaDomainException("El descuento global no puede ser negativo.");

            // Idempotencia
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var ventasExistentes = await _ventaRepository.BuscarAsync(v => v.IdempotencyKey == request.IdempotencyKey && v.IdEmpresa == idEmpresa, cancellationToken);
                var ventaExistente = ventasExistentes.FirstOrDefault();
                if (ventaExistente != null)
                {
                    return await ObtenerVentaPorIdAsync(ventaExistente.IdVenta, cancellationToken);
                }
            }

            Pedido? pedidoAsociado = null;

            // Validación de Pedido
            if (request.IdPedido.HasValue)
            {
                pedidoAsociado = await _pedidoRepository.ObtenerPorIdAsync(request.IdPedido.Value, cancellationToken);
                if (pedidoAsociado == null || pedidoAsociado.IdEmpresa != idEmpresa)
                    throw new VentaDomainException("El pedido especificado no existe o no pertenece a la empresa.");

                if (pedidoAsociado.Estado == "CANCELADO" || pedidoAsociado.Estado == "RECHAZADO" || pedidoAsociado.Estado == "ATENDIDO")
                    throw new VentaDomainException($"No se puede crear una venta a partir de un pedido en estado {pedidoAsociado.Estado}.");

                bool yaTieneVenta = await _ventaRepository.ExisteVentaParaPedidoAsync(idEmpresa, request.IdPedido.Value, cancellationToken);
                if (yaTieneVenta)
                    throw new VentaDomainException("El pedido ya se encuentra asociado a una venta.");
            }

            // Validación de Cliente
            var clientes = await _clienteRepository.BuscarAsync(c => c.IdCliente == request.IdCliente && c.IdEmpresa == idEmpresa, cancellationToken);
            var cliente = clientes.FirstOrDefault();
            if (cliente == null)
                throw new VentaDomainException("El cliente especificado no existe o no pertenece a la empresa.");
            if (!cliente.Activo)
                throw new VentaDomainException("El cliente especificado está inactivo.");

            // Validación de Sucursal
            Sucursal? sucursal = null;
            if (request.IdSucursal.HasValue)
            {
                var sucursales = await _sucursalRepository.BuscarAsync(s => s.IdSucursal == request.IdSucursal.Value && s.IdEmpresa == idEmpresa, cancellationToken);
                sucursal = sucursales.FirstOrDefault();
                if (sucursal == null)
                    throw new VentaDomainException("La sucursal especificada no existe o no pertenece a la empresa.");
                if (!sucursal.Activo)
                    throw new VentaDomainException("La sucursal especificada está inactiva.");
            }
            else 
            {
                throw new VentaDomainException("La sucursal es obligatoria para procesar el inventario.");
            }

            var fechaOperacion = DateTime.UtcNow;

            var taxRequest = new TaxEngineRequestDTO
            {
                IdEmpresa = idEmpresa,
                IdSucursal = request.IdSucursal,
                FechaOperacion = fechaOperacion,
                DescuentoGlobal = request.DescuentoGlobal,
                Lineas = new List<TaxEngineLineRequestDTO>()
            };

            var productosDict = new Dictionary<string, Producto>();
            var unidadesAplicadas = new Dictionary<string, string>();
            int index = 0;

            foreach (var reqDetalle in request.Productos)
            {
                var p = await _productoRepository.ObtenerPorIdAsync(reqDetalle.IdProducto, cancellationToken);
                if (p == null)
                    throw new VentaDomainException($"El producto con ID {reqDetalle.IdProducto} no existe.");
                
                if (!string.IsNullOrEmpty(p.Estado) && !string.Equals(p.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                    throw new VentaDomainException($"El producto '{p.NombreComercial}' no está activo.");

                var unidadMedida = await _unidadMedidaRepository.ObtenerPorIdAsync(p.IdUnidadMedida, cancellationToken);
                var codigoUnidad = unidadMedida?.Codigo?.Trim().ToUpperInvariant();
                if (unidadMedida == null || !unidadMedida.Activo || string.IsNullOrWhiteSpace(codigoUnidad))
                    throw new VentaDomainException($"El producto '{p.NombreComercial}' no tiene una unidad de medida fiscal activa y explícita.");

                string lineId = $"L{index++}";
                productosDict[lineId] = p;
                unidadesAplicadas[lineId] = codigoUnidad;

                taxRequest.Lineas.Add(new TaxEngineLineRequestDTO
                {
                    IdLinea = lineId,
                    IdProducto = p.IdProducto,
                    PrecioUnitario = p.PrecioVenta,
                    Cantidad = reqDetalle.Cantidad,
                    DescuentoLinea = 0m,
                    ClasificacionTributaria = p.ClasificacionTributaria,
                    ConceptoTributario = "IGV", // Default until we have ConceptoTributario on product
                    EsPrecioBruto = false // Pending setting or explicit boolean
                });
            }

            var importeAntesDeDescuento = taxRequest.Lineas.Sum(l => l.PrecioUnitario * l.Cantidad);
            if (request.DescuentoGlobal > importeAntesDeDescuento)
                throw new VentaDomainException("El descuento global no puede superar el importe de los productos.");

            var taxResult = await _taxEngine.CalculateTaxesAsync(taxRequest);

            var detalles = new List<VentaDetalle>();

            foreach (var reqLinea in taxRequest.Lineas)
            {
                var p = productosDict[reqLinea.IdLinea];
                var resLinea = taxResult.Lineas.First(l => l.IdLinea == reqLinea.IdLinea);

                detalles.Add(new VentaDetalle
                {
                    IdProducto = p.IdProducto,
                    Cantidad = reqLinea.Cantidad,
                    PrecioUnitario = reqLinea.PrecioUnitario, // Snapshot oficial
                    CostoUnitario = p.PrecioCompra, // Snapshot histórico del costo
                    Descuento = resLinea.DescuentoLinea,
                    BaseImponible = resLinea.BaseImponible,
                    Subtotal = resLinea.BaseImponible, // Para VentaDetalle, el subtotal histórico era BaseImponible
                    PorcentajeImpuesto = resLinea.TasaAplicada,
                    MontoImpuesto = resLinea.MontoImpuesto,
                    TotalLinea = resLinea.TotalLinea
                });
            }

            var venta = new Venta
            {
                IdEmpresa = idEmpresa,
                IdSucursal = request.IdSucursal ?? 0,
                IdCliente = request.IdCliente,
                IdPedido = request.IdPedido,
                Origen = request.Origen ?? "POS",
                Observaciones = request.Observaciones,
                IdempotencyKey = request.IdempotencyKey,
                Estado = "COMPLETADA",
                Subtotal = taxResult.TotalBaseImponible,
                Impuesto = taxResult.TotalImpuestos,
                DescuentoTotal = taxResult.TotalDescuentoGlobal,
                Total = taxResult.TotalGeneral,
                Moneda = "PEN",
                FechaVenta = fechaOperacion,
                IdUsuarioVendedor = idUsuarioVendedor,
                FechaCreacion = fechaOperacion,
                UsuarioCreacion = _currentUser.Username ?? "SISTEMA",
                VentaDetalles = detalles
            };

            // Obtener Almacén de la sucursal para descuento de inventario
            var almacenes = await _almacenRepository.BuscarAsync(a => a.IdSucursal == sucursal.IdSucursal, cancellationToken);
            var almacen = almacenes.FirstOrDefault();
            if (almacen == null)
            {
                throw new VentaDomainException("La sucursal no tiene un almacén asociado para procesar el inventario.");
            }

            var itemsDescuento = detalles.Select(d => new DescuentoStockRequest
            {
                IdProducto = d.IdProducto,
                IdAlmacen = almacen.IdAlmacen,
                IdUbicacion = 0, // El servicio lo resolverá
                Cantidad = d.Cantidad
            });

            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                // El número de venta se genera DENTRO de la transacción para garantizar
                // consistencia y evitar saltos (huecos) en caso de fallos.
                venta.NumeroVenta = await _numerador.GenerarNumeroVentaAsync(idEmpresa, request.IdSucursal);
                
                await _ventaRepository.AgregarAsync(venta, cancellationToken);
                
                if (pedidoAsociado != null)
                {
                    pedidoAsociado.Estado = "ATENDIDO";
                    pedidoAsociado.FechaProcesamiento = DateTime.UtcNow;
                    pedidoAsociado.UsuarioProcesamiento = _currentUser.Username;
                    _pedidoRepository.Actualizar(pedidoAsociado);
                }

                await _uow.GuardarCambiosAsync(cancellationToken);
                
                await _inventarioService.DescontarStockPorVentaAsync(idEmpresa, venta.IdVenta, "VENTA", itemsDescuento, cancellationToken);
                
                // Realizar un GuardarCambiosAsync adicional por si el inventario necesita persistencia de sus Movimientos antes del commit final (aunque el InventarioService no los requiere si el uow los trackea, lo dejamos por seguridad)
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey) && msg.Contains("UQ_Ventas_IdEmpresa_Idempotency"))
                {
                    var ventasExistentes = await _ventaRepository.BuscarAsync(v => v.IdempotencyKey == request.IdempotencyKey && v.IdEmpresa == idEmpresa, cancellationToken);
                    var concurrente = ventasExistentes.FirstOrDefault();
                    if (concurrente != null) return await ObtenerVentaPorIdAsync(concurrente.IdVenta, cancellationToken);
                }
                else if (request.IdPedido.HasValue && msg.Contains("UQ_Ventas_IdPedido"))
                {
                    throw new VentaDomainException("El pedido ya se encuentra asociado a otra venta concurrente.");
                }
                throw;
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return await ObtenerVentaPorIdAsync(venta.IdVenta, cancellationToken);
        }

        public async Task<VentaResponse> ObtenerVentaPorIdAsync(int idVenta, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var venta = await _ventaRepository.ObtenerVentaConDetallesAsync(idEmpresa, idVenta, cancellationToken);
            
            if (venta == null)
                throw new VentaNotFoundException(idVenta);

            return MapearAVentaResponse(venta);
        }

        public async Task<VentaResponse> ObtenerVentaPorNumeroAsync(string numeroVenta, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var venta = await _ventaRepository.ObtenerVentaPorNumeroAsync(idEmpresa, numeroVenta, cancellationToken);
            
            if (venta == null)
                throw new VentaDomainException($"La venta número {numeroVenta} no fue encontrada.");
                
            // Cargamos detalles manualmente ya que el Repositorio de ObtenerVentaPorNumeroAsync no lo trae en este diseño.
            return await ObtenerVentaPorIdAsync(venta.IdVenta, cancellationToken);
        }

        public async Task<VentaResponse> ObtenerVentaPorPedidoAsync(int idPedido, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var venta = await _ventaRepository.ObtenerVentaPorPedidoAsync(idEmpresa, idPedido, cancellationToken);
            
            if (venta == null)
                throw new VentaDomainException($"La venta asociada al pedido {idPedido} no fue encontrada.");
                
            return await ObtenerVentaPorIdAsync(venta.IdVenta, cancellationToken);
        }

        public async Task<IEnumerable<VentaListResponse>> ListarVentasAsync(VentaFilterRequest filtro, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var paginacion = await _ventaRepository.ObtenerVentasPaginadasAsync(
                idEmpresa, 
                filtro.PageNumber, 
                filtro.PageSize, 
                filtro.Estado, 
                filtro.FechaDesde, 
                filtro.FechaHasta, 
                filtro.IdSucursal, 
                filtro.IdCliente, 
                null, 
                cancellationToken);

            return paginacion.Ventas.Select(v => new VentaListResponse
            {
                IdVenta = v.IdVenta,
                NumeroVenta = v.NumeroVenta,
                Estado = v.Estado,
                Total = v.Total,
                Moneda = v.Moneda,
                FechaVenta = v.FechaVenta
            }).ToList();
        }

        public async Task AnularVentaAsync(int idVenta, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(motivo)) throw new VentaDomainException("El motivo de anulación es obligatorio.");
            
            var idEmpresa = ObtenerEmpresaActual();
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta, cancellationToken);
            
            if (venta == null || venta.IdEmpresa != idEmpresa)
                throw new VentaNotFoundException(idVenta);

            if (!venta.RowVersion.SequenceEqual(rowVersion))
                throw new VentaConcurrencyException($"La venta {idVenta} fue modificada por otro usuario u operación concurrente.");

            if (venta.Estado == "ANULADA")
                throw new VentaDomainException($"La venta {idVenta} ya se encuentra anulada.");

            venta.Estado = "ANULADA";
            venta.Observaciones = string.IsNullOrWhiteSpace(venta.Observaciones) ? $"Anulada: {motivo}" : $"{venta.Observaciones} | Anulada: {motivo}";
            venta.FechaModificacion = DateTime.UtcNow;
            venta.UsuarioModificacion = _currentUser.Username;
            
            _ventaRepository.Actualizar(venta);
            try
            {
                await _uow.GuardarCambiosAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new VentaConcurrencyException($"La venta {idVenta} fue modificada por otro usuario u operación concurrente.");
            }
        }

        private VentaResponse MapearAVentaResponse(Venta venta)
        {
            var response = new VentaResponse
            {
                IdVenta = venta.IdVenta,
                NumeroVenta = venta.NumeroVenta,
                IdEmpresa = venta.IdEmpresa,
                IdSucursal = venta.IdSucursal,
                IdCliente = venta.IdCliente,
                IdPedido = venta.IdPedido,
                Origen = venta.Origen,
                Estado = venta.Estado,
                Observaciones = venta.Observaciones,
                Subtotal = venta.Subtotal,
                Impuesto = venta.Impuesto,
                DescuentoTotal = venta.DescuentoTotal,
                Total = venta.Total,
                Moneda = venta.Moneda,
                FechaVenta = venta.FechaVenta,
                RowVersion = venta.RowVersion
            };

            if (venta.VentaDetalles != null && venta.VentaDetalles.Any())
            {
                response.Detalles = venta.VentaDetalles.Select(d => new VentaDetalleResponse
                {
                    IdDetalleVenta = d.IdDetalleVenta,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal,
                    PorcentajeImpuesto = d.PorcentajeImpuesto,
                    MontoImpuesto = d.MontoImpuesto,
                    TotalLinea = d.TotalLinea
                }).ToList();
            }

            return response;
        }
    }
}
