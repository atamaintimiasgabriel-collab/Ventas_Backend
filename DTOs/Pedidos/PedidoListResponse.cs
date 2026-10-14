namespace SistemaVentasMotorrepuestos.API.DTOs.Pedidos
{
    public class PedidoListResponse
    {
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
    }
}
