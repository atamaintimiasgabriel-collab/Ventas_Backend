namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Transferencia
    {
        public int IdTransferencia { get; set; }
        public int IdAlmacenOrigen { get; set; }
        public int IdAlmacenDestino { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; } = null!;
        public DateTime? FechaConfirmacion { get; set; }
        public string? UsuarioConfirmacion { get; set; }
        public string? Observacion { get; set; }

        public Almacen AlmacenOrigen { get; set; } = null!;
        public Almacen AlmacenDestino { get; set; } = null!;
        public ICollection<DetalleTransferencia> Detalles { get; set; } = new List<DetalleTransferencia>();
    }
}