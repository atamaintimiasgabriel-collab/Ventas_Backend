namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = null!;
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public int IdCliente { get; set; }
        public string Origen { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string? Observaciones { get; set; }
        public int? IdDireccionEntrega { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = null!;

        public DateTime FechaPedido { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
        public string? UsuarioProcesamiento { get; set; }
        public string? MotivoCancelacion { get; set; }
        public string? IdempotencyKey { get; set; }

        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        // Navegación
        public Empresa Empresa { get; set; } = null!;
        public Sucursal? Sucursal { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public ClienteDireccion? DireccionEntrega { get; set; }
        
        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}
