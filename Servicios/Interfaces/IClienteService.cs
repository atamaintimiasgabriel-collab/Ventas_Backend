using SistemaVentasMotorrepuestos.API.DTOs.Clientes;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IClienteService
    {
        /// <summary>
        /// Obtiene lista de clientes de la empresa autenticada con filtros opcionales
        /// El filtro de búsqueda se aplica a múltiples campos (documento, razón social, email, etc)
        /// La empresa activa se extrae automáticamente del contexto autenticado
        /// </summary>
        Task<IReadOnlyList<ClienteListResponse>> ListarClientesAsync(
            ClienteFilterRequest filtro,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene detalle completo de un cliente por ID
        /// Valida que el cliente pertenece a la empresa autenticada
        /// Lanza NotFoundException si no existe
        /// </summary>
        Task<ClienteDetailResponse> ObtenerClientePorIdAsync(
            int idCliente,
            CancellationToken cancellationToken = default);

        Task<ClienteDetailResponse> CrearClienteAsync(
            CrearClienteRequest request,
            CancellationToken cancellationToken = default);
        Task<ClienteDetailResponse> ActualizarClienteAsync(int idCliente, ActualizarClienteRequest request, CancellationToken cancellationToken = default);
        Task<ClienteDetailResponse> CambiarEstadoAsync(int idCliente, bool activo, CancellationToken cancellationToken = default);
        Task<ClienteStatsResponse> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
        Task<ClientePagedResponse> ListarClientesPaginadosAsync(ClienteFilterRequest filtro, CancellationToken cancellationToken = default);
    }
}
