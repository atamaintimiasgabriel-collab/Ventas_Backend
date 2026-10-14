namespace SistemaVentasMotorrepuestos.API.DTOs.Pedidos
{
    public class PedidoResponse
    {
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public int IdCliente { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public int? IdDireccionEntrega { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
        public string? MotivoCancelacion { get; set; }
        
        public byte[]? RowVersion { get; set; }

        public List<PedidoDetalleResponse> Detalles { get; set; } = new();
    }
}
