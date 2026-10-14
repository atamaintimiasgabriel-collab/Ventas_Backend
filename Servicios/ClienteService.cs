using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.Clientes;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    /// <summary>
    /// ClienteService: Capa de negocio para gestión de clientes
    /// Aplica tenant isolation automáticamente vía ICurrentUserService.IdEmpresa
    /// No acepta IdEmpresa como parámetro desde la request
    /// </summary>
    public class ClienteService : IClienteService
    {
        private readonly IRepositorioBase<Cliente> _clienteRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IRepositorioBase<Venta> _ventaRepository;
        private readonly Datos.ApplicationDbContext _db;

        public ClienteService(
            IRepositorioBase<Cliente> clienteRepository,
            ICurrentUserService currentUser,
            IRepositorioBase<Venta> ventaRepository,
            Datos.ApplicationDbContext db)
        {
            _clienteRepository = clienteRepository;
            _currentUser = currentUser;
            _ventaRepository = ventaRepository;
            _db = db;
        }

        /// <summary>
        /// Extrae la empresa actual del contexto autenticado
        /// Lanza excepción si el usuario no tiene empresa asignada
        /// </summary>
        private int ObtenerEmpresaActual()
        {
            var idEmpresa = _currentUser.IdEmpresa;
            if (idEmpresa == null || idEmpresa <= 0)
                throw new PedidoDomainException("El usuario autenticado no tiene una empresa asignada o el token es inválido.");
            return idEmpresa.Value;
        }

        /// <summary>
        /// Mapea Cliente model → ClienteListResponse
        /// </summary>
        private ClienteListResponse MapearAListResponse(Cliente cliente)
        {
            return new ClienteListResponse
            {
                IdCliente = cliente.IdCliente,
                TipoDocumento = cliente.TipoDocumento,
                NumeroDocumento = cliente.NumeroDocumento,
                RazonSocial = cliente.RazonSocial,
                NombreComercial = cliente.NombreComercial,
                Nombres = cliente.Nombres,
                Apellidos = cliente.Apellidos,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Activo = cliente.Activo,
                FechaCreacion = cliente.FechaCreacion
                ,NombreCompleto = string.Join(" ", new[] { cliente.RazonSocial ?? cliente.NombreComercial, cliente.Nombres, cliente.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x)))
            };
        }

        /// <summary>
        /// Mapea Cliente model → ClienteDetailResponse
        /// </summary>
        private ClienteDetailResponse MapearADetailResponse(Cliente cliente)
        {
            return new ClienteDetailResponse
            {
                IdCliente = cliente.IdCliente,
                TipoPersona = cliente.TipoPersona,
                TipoDocumento = cliente.TipoDocumento,
                NumeroDocumento = cliente.NumeroDocumento,
                RazonSocial = cliente.RazonSocial,
                NombreComercial = cliente.NombreComercial,
                Nombres = cliente.Nombres,
                Apellidos = cliente.Apellidos,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                LimiteCredito = cliente.LimiteCredito,
                Activo = cliente.Activo,
                FechaCreacion = cliente.FechaCreacion,
                UsuarioCreacion = cliente.UsuarioCreacion,
                FechaModificacion = cliente.FechaModificacion,
                UsuarioModificacion = cliente.UsuarioModificacion
            };
        }

        /// <summary>
        /// ListarClientesAsync: Obtiene clientes de la empresa autenticada con filtros
        /// Aplicar tenant isolation: c.IdEmpresa == idEmpresa
        /// No aceptar IdEmpresa desde cliente
        /// </summary>
        public async Task<IReadOnlyList<ClienteListResponse>> ListarClientesAsync(
            ClienteFilterRequest filtro,
            CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();

            // Construir predicado dinámico
            var clientes = await _clienteRepository.BuscarAsync(
                c => c.IdEmpresa == idEmpresa
                    && (string.IsNullOrWhiteSpace(filtro.Buscar)
                        || c.NumeroDocumento.Contains(filtro.Buscar)
                        || (c.RazonSocial != null && c.RazonSocial.Contains(filtro.Buscar))
                        || (c.NombreComercial != null && c.NombreComercial.Contains(filtro.Buscar))
                        || (c.Nombres != null && c.Nombres.Contains(filtro.Buscar))
                        || (c.Apellidos != null && c.Apellidos.Contains(filtro.Buscar))
                        || (c.Telefono != null && c.Telefono.Contains(filtro.Buscar))
                        || (c.Email != null && c.Email.Contains(filtro.Buscar)))
                    && (string.IsNullOrWhiteSpace(filtro.TipoPersona) || c.TipoPersona == filtro.TipoPersona)
                    && (string.IsNullOrWhiteSpace(filtro.TipoDocumento) || c.TipoDocumento == filtro.TipoDocumento)
                    && (!filtro.Activo.HasValue || c.Activo == filtro.Activo.Value),
                cancellationToken);

            // Aplicar paginación
            var clientesPaginados = clientes
                .OrderByDescending(c => c.FechaCreacion)
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            return clientesPaginados.Select(MapearAListResponse).ToList().AsReadOnly();
        }

        /// <summary>
        /// ObtenerClientePorIdAsync: Obtiene detalle de un cliente validando tenant
        /// </summary>
        public async Task<ClienteDetailResponse> ObtenerClientePorIdAsync(
            int idCliente,
            CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();

            var clientes = await _clienteRepository.BuscarAsync(
                c => c.IdCliente == idCliente && c.IdEmpresa == idEmpresa,
                cancellationToken);

            var cliente = clientes.FirstOrDefault();
            if (cliente == null)
                throw new PedidoDomainException("El cliente especificado no existe o no pertenece a la empresa.");

            return MapearADetailResponse(cliente);
        }

        public async Task<ClienteDetailResponse> CrearClienteAsync(
            CrearClienteRequest request,
            CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var tipoPersona = request.TipoPersona.Trim().ToUpperInvariant();
            var tipoDocumento = request.TipoDocumento.Trim().ToUpperInvariant();
            var numeroDocumento = request.NumeroDocumento.Trim();

            if (tipoPersona != "N" && tipoPersona != "J")
                throw new PedidoDomainException("TipoPersona debe ser N o J.");

            if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
                throw new PedidoDomainException("TipoDocumento y NumeroDocumento son obligatorios.");

            if (tipoPersona == "N" && string.IsNullOrWhiteSpace(request.Nombres))
                throw new PedidoDomainException("Nombres es obligatorio para persona natural.");

            if (tipoPersona == "J" && string.IsNullOrWhiteSpace(request.RazonSocial))
                throw new PedidoDomainException("RazonSocial es obligatoria para persona juridica.");

            var existentes = await _clienteRepository.BuscarAsync(
                c => c.IdEmpresa == idEmpresa
                    && c.TipoDocumento == tipoDocumento
                    && c.NumeroDocumento == numeroDocumento,
                cancellationToken);

            if (existentes.Any())
                throw new PedidoDomainException("Ya existe un cliente con el mismo documento en la empresa actual.");

            var cliente = new Cliente
            {
                IdEmpresa = idEmpresa,
                TipoPersona = tipoPersona,
                TipoDocumento = tipoDocumento,
                NumeroDocumento = numeroDocumento,
                RazonSocial = string.IsNullOrWhiteSpace(request.RazonSocial) ? null : request.RazonSocial.Trim(),
                NombreComercial = string.IsNullOrWhiteSpace(request.NombreComercial) ? null : request.NombreComercial.Trim(),
                Nombres = string.IsNullOrWhiteSpace(request.Nombres) ? null : request.Nombres.Trim(),
                Apellidos = string.IsNullOrWhiteSpace(request.Apellidos) ? null : request.Apellidos.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim(),
                LimiteCredito = 0m,
                Activo = true,
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = _currentUser.Username
            };

            await _clienteRepository.AgregarAsync(cliente, cancellationToken);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);

            return MapearADetailResponse(cliente);
        }

        public async Task<ClienteDetailResponse> ActualizarClienteAsync(int idCliente, ActualizarClienteRequest request, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var cliente = (await _clienteRepository.BuscarAsync(c => c.IdCliente == idCliente && c.IdEmpresa == idEmpresa, cancellationToken)).FirstOrDefault();
            if (cliente == null) throw new PedidoDomainException("El cliente especificado no existe o no pertenece a la empresa.");
            cliente.RazonSocial = string.IsNullOrWhiteSpace(request.RazonSocial) ? null : request.RazonSocial.Trim();
            cliente.NombreComercial = string.IsNullOrWhiteSpace(request.NombreComercial) ? null : request.NombreComercial.Trim();
            cliente.Nombres = string.IsNullOrWhiteSpace(request.Nombres) ? null : request.Nombres.Trim();
            cliente.Apellidos = string.IsNullOrWhiteSpace(request.Apellidos) ? null : request.Apellidos.Trim();
            cliente.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            cliente.Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim();
            _clienteRepository.Actualizar(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            return MapearADetailResponse(cliente);
        }

        public async Task<ClienteDetailResponse> CambiarEstadoAsync(int idCliente, bool activo, CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var cliente = (await _clienteRepository.BuscarAsync(c => c.IdCliente == idCliente && c.IdEmpresa == idEmpresa, cancellationToken)).FirstOrDefault();
            if (cliente == null) throw new PedidoDomainException("El cliente especificado no existe o no pertenece a la empresa.");
            cliente.Activo = activo;
            _clienteRepository.Actualizar(cliente);
            await _clienteRepository.GuardarCambiosAsync(cancellationToken);
            return MapearADetailResponse(cliente);
        }

        public async Task<ClienteStatsResponse> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            var idEmpresa = ObtenerEmpresaActual();
            var clientes = (await _clienteRepository.BuscarAsync(c => c.IdEmpresa == idEmpresa, cancellationToken)).ToList();
            var ventas = (await _ventaRepository.BuscarAsync(v => v.IdEmpresa == idEmpresa, cancellationToken)).ToList();
            var ahora = DateTime.Now;
            return new ClienteStatsResponse
            {
                Total = clientes.Count,
                Activos = clientes.Count(c => c.Activo),
                Inactivos = clientes.Count(c => !c.Activo),
                NuevosDelMes = clientes.Count(c => c.FechaCreacion.HasValue && c.FechaCreacion.Value.Year == ahora.Year && c.FechaCreacion.Value.Month == ahora.Month),
                TotalCompras = ventas.Where(v => !string.Equals(v.Estado, "ANULADA", StringComparison.OrdinalIgnoreCase) && !string.Equals(v.Estado, "ANULADO", StringComparison.OrdinalIgnoreCase)).Sum(v => v.Total),
                Crecimiento = clientes.Where(c => c.FechaCreacion.HasValue).GroupBy(c => new { c.FechaCreacion!.Value.Year, c.FechaCreacion!.Value.Month }).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).TakeLast(12).Select(g => new ClienteGrowthPoint { Periodo = $"{g.Key.Year}-{g.Key.Month:00}", Total = g.Count() }).ToList()
            };
        }

        public async Task<ClientePagedResponse> ListarClientesPaginadosAsync(ClienteFilterRequest filtro, CancellationToken cancellationToken = default)
        {
            filtro.PageNumber = Math.Max(1, filtro.PageNumber);
            filtro.PageSize = Math.Clamp(filtro.PageSize, 1, 100);
            var empresa = ObtenerEmpresaActual();
            var query = _db.Clientes.AsNoTracking().Where(c => c.IdEmpresa == empresa);
            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var buscar = filtro.Buscar.Trim();
                query = query.Where(c => c.NumeroDocumento.Contains(buscar) || (c.RazonSocial != null && c.RazonSocial.Contains(buscar)) || (c.NombreComercial != null && c.NombreComercial.Contains(buscar)) || (c.Nombres != null && c.Nombres.Contains(buscar)) || (c.Apellidos != null && c.Apellidos.Contains(buscar)) || (c.Telefono != null && c.Telefono.Contains(buscar)) || (c.Email != null && c.Email.Contains(buscar)));
            }
            if (!string.IsNullOrWhiteSpace(filtro.TipoPersona)) query = query.Where(c => c.TipoPersona == filtro.TipoPersona);
            if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento)) query = query.Where(c => c.TipoDocumento == filtro.TipoDocumento);
            if (filtro.Activo.HasValue) query = query.Where(c => c.Activo == filtro.Activo.Value);
            var total = await query.CountAsync(cancellationToken);
            var rows = await query.OrderByDescending(c => c.FechaCreacion).ThenBy(c => c.IdCliente).Skip((filtro.PageNumber - 1) * filtro.PageSize).Take(filtro.PageSize).ToListAsync(cancellationToken);
            var items = rows.Select(MapearAListResponse).ToList().AsReadOnly();
            return new ClientePagedResponse { Items = items, PageNumber = filtro.PageNumber, PageSize = filtro.PageSize, TotalItems = total, TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)filtro.PageSize) };
        }
    }
}
