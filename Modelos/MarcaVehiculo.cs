namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class MarcaVehiculo
    {
        public int IdMarcaVehiculo { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }

        public ICollection<ModeloVehiculo> Modelos { get; set; } = new List<ModeloVehiculo>();
    }
}