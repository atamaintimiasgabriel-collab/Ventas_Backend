namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class CompatibilidadProducto
    {
        public int IdCompatibilidad { get; set; }
        public int IdProducto { get; set; }
        public int IdVersionVehiculo { get; set; }
        public string? Observacion { get; set; }

        public Producto Producto { get; set; } = null!;
        public VersionVehiculo VersionVehiculo { get; set; } = null!;
    }
}