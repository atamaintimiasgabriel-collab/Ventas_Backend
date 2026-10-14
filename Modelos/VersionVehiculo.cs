namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class VersionVehiculo
    {
        public int IdVersionVehiculo { get; set; }
        public int IdModeloVehiculo { get; set; }
        public int? AñoInicio { get; set; }
        public int? AñoFin { get; set; }
        public string? ConfiguracionMotor { get; set; }
        public bool Activo { get; set; }

        public ModeloVehiculo ModeloVehiculo { get; set; } = null!;
        public ICollection<CompatibilidadProducto> Compatibilidades { get; set; } = new List<CompatibilidadProducto>();
    }
}