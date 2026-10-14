namespace SistemaVentasMotorrepuestos.API.DTOs.Inventario
{
    /// <summary>
    /// Response DTO para resumen de inventario
    /// Vista agregada por producto y sucursal/almacén
    /// </summary>
    public class InventarioResumenResponse
    {
        public int IdProducto { get; set; }
        public string SKU { get; set; } = null!;
        public string NombreProducto { get; set; } = null!;
        public string NomeCategoria { get; set; } = null!;
        public string NombreMarca { get; set; } = null!;
        public decimal StockTotalDisponible { get; set; }
        public decimal StockTotalReservado { get; set; }
        public decimal StockMinimo { get; set; }
        public string EstadoStock { get; set; } = "DISPONIBLE"; // DISPONIBLE, BAJO, AGOTADO
        public int CantidadUbicaciones { get; set; }
    }
}
