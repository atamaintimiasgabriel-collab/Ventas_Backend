namespace SistemaVentasMotorrepuestos.API.DTOs.Productos
{
    /// <summary>
    /// Response DTO para listado de productos en catálogo
    /// Utilizado por GET /api/v1/productos
    /// </summary>
    public class ProductoListResponse
    {
        public int IdProducto { get; set; }
        public string SKU { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public string? CodigoOEM { get; set; }
        public string NombreComercial { get; set; } = null!;
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = null!;
        public int IdMarca { get; set; }
        public string NombreMarca { get; set; } = null!;
        public int IdUnidadMedida { get; set; }
        public string CodigoUnidadMedida { get; set; } = null!;
        public decimal PrecioVenta { get; set; }
        public decimal PrecioCompra { get; set; }
        public string? Estado { get; set; }
        public string? RutaImagen { get; set; }
        public string? DescripcionTecnica { get; set; }
        public int StockDisponible { get; set; }
        
        // Nuevos campos para catálogos y filtros
        public bool EsOferta { get; set; }
        public decimal? PrecioOferta { get; set; }
        public bool EsNuevoIngreso { get; set; }
        public bool EsRecomendado { get; set; }
    }
}
