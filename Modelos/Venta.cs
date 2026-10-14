namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public int IdCliente { get; set; }
        public int? IdPedido { get; set; }
        public string NumeroVenta { get; set; } = null!;
        public string Origen { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Moneda { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal DescuentoTotal { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaVenta { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
        public int IdUsuarioVendedor { get; set; }
        public string? Observaciones { get; set; }
        public string? IdempotencyKey { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Sucursal Sucursal { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!;
        public Pedido? Pedido { get; set; }
        
        public ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
    }
}
