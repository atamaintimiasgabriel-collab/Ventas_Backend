namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ModeloVehiculo
    {
        public int IdModeloVehiculo { get; set; }
        public int IdMarcaVehiculo { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Cilindrada { get; set; }
        public bool Activo { get; set; }

        public MarcaVehiculo MarcaVehiculo { get; set; } = null!;
        public ICollection<VersionVehiculo> Versiones { get; set; } = new List<VersionVehiculo>();
    }
}