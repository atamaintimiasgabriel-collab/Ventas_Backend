namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class UnidadMedida
    {
        public int IdUnidadMedida { get; set; }
        public string Codigo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool Activo { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}