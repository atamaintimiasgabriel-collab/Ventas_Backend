namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    public class ClientePagedResponse
    {
        public IReadOnlyList<ClienteListResponse> Items { get; set; } = Array.Empty<ClienteListResponse>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
