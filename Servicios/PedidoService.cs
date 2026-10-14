using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IRepositorioBase<Producto> _productoRepository;
        private readonly IRepositorioBase<Cliente> _clienteRepository;
        private readonly IRepositorioBase<Sucursal> _sucursalRepository;
        private readonly IRepositorioBase<ClienteDireccion> _direccionRepository;
        private readonly IUnidadDeTrabajo _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly INumeradorPedido _numerador;
        private readonly ITaxEngine _taxEngine;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IRepositorioBase<Producto> productoRepository,
            IRepositorioBase<Cliente> clienteRepository,
            IRepositorioBase<Sucursal> sucursalRepository,
            IRepositorioBase<ClienteDireccion> direccionRepository,
            IUnidadDeTrabajo uow,
            ICurrentUserService currentUser,
            INumeradorPedido numerador,
            ITaxEngine taxEngine)
        {
            _pedidoRepository = pedidoRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;
            _sucursalRepository = sucursalRepository;
            _direccionRepository = direccionRepository;
            _uow = uow;
            _currentUser = currentUser;
            _numerador = numerador;
            _taxEngine = taxEngine;
        }


        private int ObtenerEmpresaActual()
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                throw new PedidoDomainException("El usuario autenticado no tiene una empresa asignada o el token es inválido.");
            return idEmpresa.Value;
        }

        public async Task<PedidoResponse> CrearPedidoAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var pedidoExistente = await _pedidoRepository.BuscarPorIdempotencyKeyAsync(idEmpresa, request.IdempotencyKey, cancellationToken);
                if (pedidoExistente != null)
                {
                    return MapearAPedidoResponse(pedidoExistente);
                }
            }

            var clientes = await _clienteRepository.BuscarAsync(c => c.IdCliente == request.IdCliente && c.IdEmpresa == idEmpresa, cancellationToken);
            var cliente = clientes.FirstOrDefault();
            if (cliente == null)
                throw new PedidoDomainException("El cliente especificado no existe o no pertenece a la empresa.");
            if (!cliente.Activo)
                throw new PedidoDomainException("El cliente especificado está inactivo.");

            if (request.IdSucursal.HasValue)
            {
                var sucursales = await _sucursalRepository.BuscarAsync(s => s.IdSucursal == request.IdSucursal.Value && s.IdEmpresa == idEmpresa, cancellationToken);
                var sucursal = sucursales.FirstOrDefault();
                if (sucursal == null)
                    throw new PedidoDomainException("La sucursal especificada no existe o no pertenece a la empresa.");
                if (!sucursal.Activo)
                    throw new PedidoDomainException("La sucursal especificada está inactiva.");
            }

            if (request.IdDireccionEntrega.HasValue)
            {
                var direcciones = await _direccionRepository.BuscarAsync(d => d.IdDireccion == request.IdDireccionEntrega.Value && d.IdCliente == request.IdCliente, cancellationToken);
                var direccion = direcciones.FirstOrDefault();
                if (direccion == null)
                    throw new PedidoDomainException("La dirección especificada no existe o no pertenece al cliente.");
                if (!direccion.Activo)
                    throw new PedidoDomainException("La dirección especificada está inactiva.");
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
            int index = 0;

            foreach (var reqDetalle in request.Productos)
            {
                var p = await _productoRepository.ObtenerPorIdAsync(reqDetalle.IdProducto, cancellationToken);
                if (p == null)
                    throw new PedidoDomainException($"El producto con ID {reqDetalle.IdProducto} no existe.");
                
                if (!string.IsNullOrEmpty(p.Estado) && !string.Equals(p.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                    throw new PedidoDomainException($"El producto '{p.NombreComercial}' no está activo.");

                string lineId = $"L{index++}";
                productosDict[lineId] = p;

                taxRequest.Lineas.Add(new TaxEngineLineRequestDTO
                {
                    IdLinea = lineId,
                    IdProducto = p.IdProducto,
                    PrecioUnitario = p.PrecioVenta,
                    Cantidad = reqDetalle.Cantidad,
                    DescuentoLinea = 0m,
                    ClasificacionTributaria = p.ClasificacionTributaria,
                    ConceptoTributario = "IGV", // Default until we have ConceptoTributario on product
                    EsPrecioBruto = false // Pending setting or explicit boolean. For now assumed false to match previous * (18/100).
                });
            }

            var taxResult = await _taxEngine.CalculateTaxesAsync(taxRequest);

            var detalles = new List<PedidoDetalle>();

            foreach (var reqLinea in taxRequest.Lineas)
            {
                var p = productosDict[reqLinea.IdLinea];
                var resLinea = taxResult.Lineas.First(l => l.IdLinea == reqLinea.IdLinea);

                detalles.Add(new PedidoDetalle
                {
                    IdProducto = p.IdProducto,
                    Cantidad = reqLinea.Cantidad,
                    PrecioUnitario = reqLinea.PrecioUnitario,
                    Descuento = resLinea.DescuentoLinea,
                    BaseImponible = resLinea.BaseImponible,
                    ClasificacionTributariaAplicada = resLinea.ClasificacionTributariaAplicada,
                    ConceptoTributarioAplicado = resLinea.ConceptoTributarioAplicado,
                    Subtotal = resLinea.BaseImponible, // Para PedidoDetalle, el subtotal histórico era BaseImponible
                    PorcentajeImpuesto = resLinea.TasaAplicada,
                    MontoImpuesto = resLinea.MontoImpuesto,
                    TotalLinea = resLinea.TotalLinea
                });
            }

            var pedido = new Pedido
            {
                IdEmpresa = idEmpresa,
                IdSucursal = request.IdSucursal,
                IdCliente = request.IdCliente,
                IdDireccionEntrega = request.IdDireccionEntrega,
                Origen = request.Origen ?? "API",
                Observaciones = request.Observaciones,
                IdempotencyKey = request.IdempotencyKey,
                NumeroPedido = _numerador.GenerarNumeroPedido(idEmpresa, request.IdSucursal),
                Estado = "PENDIENTE",
                Subtotal = taxResult.TotalBaseImponible,
                Impuesto = taxResult.TotalImpuestos,
                DescuentoTotal = taxResult.TotalDescuentoGlobal,
                Total = taxResult.TotalGeneral,
                Moneda = "PEN",
                FechaPedido = fechaOperacion,
                Detalles = detalles
            };

            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                await _pedidoRepository.AgregarAsync(pedido, cancellationToken);
                await _uow.GuardarCambiosAsync(cancellationToken);
                await _uow.CommitTransactionAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (!string.IsNullOrWhiteSpace(request.IdempotencyKey) && msg.Contains("UQ_Pedidos_IdEmpresa_Idempotency"))
                {
                    var pedidoConcurrente = await _pedidoRepository.BuscarPorIdempotencyKeyAsync(idEmpresa, request.IdempotencyKey, cancellationToken);
                    if (pedidoConcurrente != null) return MapearAPedidoResponse(pedidoConcurrente);
                }
                throw;
            }
            catch
            {
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return MapearAPedidoResponse(pedido);
        }

        public async Task<PedidoResponse> ObtenerPedidoPorIdAsync(int idPedido, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var pedido = await _pedidoRepository.ObtenerPedidoConDetallesAsync(idPedido, idEmpresa, cancellationToken);
            
            if (pedido == null)
                throw new PedidoNotFoundException(idPedido);

            return MapearAPedidoResponse(pedido);
        }

        public async Task<IEnumerable<PedidoListResponse>> ListarPedidosAsync(PedidoFilterRequest filtro, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            
            // Asumimos que ListarPedidos usará un query directo o IQueryable en una implementación avanzada.
            // Para cumplir con el Repository Base, usamos BuscarAsync que soporta IQueryable subyacente.
            // Idealmente se crearía un método en IPedidoRepository que retorne el DTO directo.
            
            var pedidos = await _pedidoRepository.BuscarAsync(p => p.IdEmpresa == idEmpresa && 
                                                                  (!filtro.IdCliente.HasValue || p.IdCliente == filtro.IdCliente.Value) &&
                                                                  (!filtro.IdSucursal.HasValue || p.IdSucursal == filtro.IdSucursal.Value) &&
                                                                  (string.IsNullOrEmpty(filtro.Estado) || p.Estado == filtro.Estado) &&
                                                                  (!filtro.FechaDesde.HasValue || p.FechaPedido >= filtro.FechaDesde.Value) &&
                                                                  (!filtro.FechaHasta.HasValue || p.FechaPedido <= filtro.FechaHasta.Value), 
                                                                  cancellationToken);

            var queryPaginada = pedidos.OrderByDescending(p => p.FechaPedido)
                                       .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                                       .Take(filtro.PageSize);

            return queryPaginada.Select(p => new PedidoListResponse
            {
                IdPedido = p.IdPedido,
                NumeroPedido = p.NumeroPedido,
                Estado = p.Estado,
                Total = p.Total,
                Moneda = p.Moneda,
                FechaPedido = p.FechaPedido
            }).ToList();
        }

        public async Task CambiarEstadoPedidoAsync(int idPedido, string nuevoEstado, byte[] rowVersion, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido, cancellationToken);
            
            if (pedido == null || pedido.IdEmpresa != idEmpresa)
                throw new PedidoNotFoundException(idPedido);

            if (!pedido.RowVersion.SequenceEqual(rowVersion))
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");

            ValidarTransicionEstado(pedido.Estado, nuevoEstado);

            pedido.Estado = nuevoEstado;
            pedido.Estado = nuevoEstado;
            
            if (nuevoEstado == "EN_PROCESO" || nuevoEstado == "CONFIRMADO" || nuevoEstado == "ATENDIDO")
            {
                pedido.FechaProcesamiento = DateTime.UtcNow;
                pedido.UsuarioProcesamiento = _currentUser.Username;
            }

            _pedidoRepository.Actualizar(pedido);
            // EF Core requiere que modifiquemos el OriginalValue del RowVersion para la comprobación optimista
            // Dado que no tenemos el DbContext aquí, debemos depender de que el repositorio asigne el valor original si la entidad ya está trackeada.
            // Para simplificar, en Arquitecturas limpias se expone un método en el repositorio.
            // Como esto rompe la fase si modifico la interfaz, lo manejaré capturando la validación aquí en el test.
            try
            {
                await _uow.GuardarCambiosAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");
            }
        }

        public async Task CancelarPedidoAsync(int idPedido, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(motivo)) throw new PedidoDomainException("El motivo de cancelación es obligatorio.");
            
            var idEmpresa = ObtenerEmpresaActual();
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido, cancellationToken);
            
            if (pedido == null || pedido.IdEmpresa != idEmpresa)
                throw new PedidoNotFoundException(idPedido);

            if (!pedido.RowVersion.SequenceEqual(rowVersion))
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");

            ValidarTransicionEstado(pedido.Estado, "CANCELADO");

            pedido.Estado = "CANCELADO";
            pedido.MotivoCancelacion = motivo;
            pedido.RowVersion = rowVersion;

            _pedidoRepository.Actualizar(pedido);
            try
            {
                await _uow.GuardarCambiosAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");
            }
        }

        public async Task RechazarPedidoAsync(int idPedido, string motivo, byte[] rowVersion, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(motivo)) throw new PedidoDomainException("El motivo de rechazo es obligatorio.");
            
            var idEmpresa = ObtenerEmpresaActual();
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido, cancellationToken);
            
            if (pedido == null || pedido.IdEmpresa != idEmpresa)
                throw new PedidoNotFoundException(idPedido);

            if (!pedido.RowVersion.SequenceEqual(rowVersion))
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");

            ValidarTransicionEstado(pedido.Estado, "RECHAZADO");

            pedido.Estado = "RECHAZADO";
            pedido.MotivoCancelacion = motivo; // Se utiliza el mismo campo para detallar el motivo
            pedido.RowVersion = rowVersion;

            _pedidoRepository.Actualizar(pedido);
            try
            {
                await _uow.GuardarCambiosAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new PedidoConcurrencyException($"El pedido {idPedido} fue modificado por otro usuario u operación concurrente.");
            }
        }

        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            // Máquina de estados
            // PENDIENTE -> EN_PROCESO, CANCELADO, RECHAZADO
            // EN_PROCESO -> CONFIRMADO, CANCELADO, RECHAZADO
            // CONFIRMADO -> ATENDIDO, CANCELADO, RECHAZADO
            // ATENDIDO, CANCELADO, RECHAZADO -> (estados terminales, no cambian)
            
            if (estadoActual == "ATENDIDO" || estadoActual == "CANCELADO" || estadoActual == "RECHAZADO")
                throw new PedidoDomainException($"El pedido se encuentra en un estado terminal ({estadoActual}) y no puede ser modificado.");

            bool esTransicionValida = false;

            if (estadoActual == "PENDIENTE" && (nuevoEstado == "EN_PROCESO" || nuevoEstado == "CANCELADO" || nuevoEstado == "RECHAZADO"))
                esTransicionValida = true;
            else if (estadoActual == "EN_PROCESO" && (nuevoEstado == "CONFIRMADO" || nuevoEstado == "CANCELADO" || nuevoEstado == "RECHAZADO"))
                esTransicionValida = true;
            else if (estadoActual == "CONFIRMADO" && (nuevoEstado == "ATENDIDO" || nuevoEstado == "CANCELADO" || nuevoEstado == "RECHAZADO"))
                esTransicionValida = true;

            if (!esTransicionValida)
                throw new PedidoDomainException($"No se permite la transición de estado desde {estadoActual} hacia {nuevoEstado}.");
        }

        private PedidoResponse MapearAPedidoResponse(Pedido pedido)
        {
            var response = new PedidoResponse
            {
                IdPedido = pedido.IdPedido,
                NumeroPedido = pedido.NumeroPedido,
                IdEmpresa = pedido.IdEmpresa,
                IdSucursal = pedido.IdSucursal,
                IdCliente = pedido.IdCliente,
                Origen = pedido.Origen,
                Estado = pedido.Estado,
                Observaciones = pedido.Observaciones,
                IdDireccionEntrega = pedido.IdDireccionEntrega,
                Subtotal = pedido.Subtotal,
                Impuesto = pedido.Impuesto,
                DescuentoTotal = pedido.DescuentoTotal,
                Total = pedido.Total,
                Moneda = pedido.Moneda,
                FechaPedido = pedido.FechaPedido,
                FechaProcesamiento = pedido.FechaProcesamiento,
                MotivoCancelacion = pedido.MotivoCancelacion,
                RowVersion = pedido.RowVersion
            };

            if (pedido.Detalles != null && pedido.Detalles.Any())
            {
                response.Detalles = pedido.Detalles.Select(d => new PedidoDetalleResponse
                {
                    IdDetallePedido = d.IdDetallePedido,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    Subtotal = d.Subtotal,
                    PorcentajeImpuesto = d.PorcentajeImpuesto,
                    MontoImpuesto = d.MontoImpuesto
                }).ToList();
            }

            return response;
        }
    }
}
