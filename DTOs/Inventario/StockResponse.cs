namespace SistemaVentasMotorrepuestos.API.DTOs.Inventario
{
    /// <summary>
    /// Response DTO para información de stock de un producto
    /// Detalle de inventario por ubicación y almacén
    /// </summary>
    public class StockResponse
    {
        public int IdStock { get; set; }
        public int IdProducto { get; set; }
        public string SKUProducto { get; set; } = null!;
        public string NombreProducto { get; set; } = null!;
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = null!;
        public int IdUbicacion { get; set; }
        public string CodigoUbicacion { get; set; } = null!;
        public decimal StockFisico { get; set; }
        public decimal StockReservado { get; set; }
        public decimal StockSeguridad { get; set; }
        public decimal StockDisponible => StockFisico - StockReservado - StockSeguridad;
        public decimal StockMinimo { get; set; }
    }
}
