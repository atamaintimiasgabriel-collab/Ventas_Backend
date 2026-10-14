namespace SistemaVentasMotorrepuestos.API.DTOs.Productos
{
    public class ProductoRequest
    {
        public string SKU { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public string? CodigoOEM { get; set; }
        public string NombreComercial { get; set; } = null!;
        public string? DescripcionTecnica { get; set; }
        public int IdCategoria { get; set; }
        public int? IdSubcategoria { get; set; }
        public int IdMarca { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal? MargenGanancia { get; set; }
        public string? Estado { get; set; }
        public string? RutaImagen { get; set; }
        public string ClasificacionTributaria { get; set; } = "GRA";
        public bool EsOferta { get; set; } = false;
        public decimal? PrecioOferta { get; set; }
        public bool EsNuevoIngreso { get; set; } = false;
        public bool EsRecomendado { get; set; } = false;
    }
}
