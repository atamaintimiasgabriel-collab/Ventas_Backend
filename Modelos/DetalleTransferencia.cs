namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class DetalleTransferencia
    {
        public int IdDetalleTransferencia { get; set; }
        public int IdTransferencia { get; set; }
        public int IdAlmacenOrigen { get; set; }
        public int IdAlmacenDestino { get; set; }
        public int IdProducto { get; set; }
        public int IdUbicacionOrigen { get; set; }
        public int IdUbicacionDestino { get; set; }
        public decimal Cantidad { get; set; }

        public Transferencia Transferencia { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
        public Ubicacion UbicacionOrigen { get; set; } = null!;
        public Ubicacion UbicacionDestino { get; set; } = null!;
    }
}