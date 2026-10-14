namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ProductoEquivalente
    {
        public int IdProductoPrincipal { get; set; }
        public int IdProductoSustituto { get; set; }

        public Producto ProductoPrincipal { get; set; } = null!;
        public Producto ProductoSustituto { get; set; } = null!;
    }
}