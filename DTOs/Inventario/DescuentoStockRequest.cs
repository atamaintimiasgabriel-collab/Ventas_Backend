namespace SistemaVentasMotorrepuestos.API.DTOs.Inventario
{
    public class DescuentoStockRequest
    {
        public int IdProducto { get; set; }
        public int IdAlmacen { get; set; }
        public int IdUbicacion { get; set; }
        public decimal Cantidad { get; set; }
    }
}
