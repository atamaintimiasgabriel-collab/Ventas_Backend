namespace SistemaVentasMotorrepuestos.API.DTOs.Pedidos
{
    public class CambiarEstadoPedidoRequest
    {
        public string NuevoEstado { get; set; } = null!;
        public string? Motivo { get; set; }
    }
}
