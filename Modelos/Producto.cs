namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Producto
    {
        public int IdProducto { get; set; }
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
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        public string ClasificacionTributaria { get; set; } = "GRA";

        public bool EsOferta { get; set; } = false;
        public decimal? PrecioOferta { get; set; }
        public bool EsNuevoIngreso { get; set; } = false;
        public bool EsRecomendado { get; set; } = false;

        public Categoria Categoria { get; set; } = null!;
        public Subcategoria? Subcategoria { get; set; }
        public Marca Marca { get; set; } = null!;
        public UnidadMedida UnidadMedida { get; set; } = null!;
        
        public ICollection<CompatibilidadProducto> Compatibilidades { get; set; } = new List<CompatibilidadProducto>();
        public ICollection<ProductoEquivalente> EquivalentesPrincipales { get; set; } = new List<ProductoEquivalente>();
        public ICollection<ProductoEquivalente> EquivalentesSustitutos { get; set; } = new List<ProductoEquivalente>();
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
        public ICollection<DetalleTransferencia> DetallesTransferencia { get; set; } = new List<DetalleTransferencia>();
        public ICollection<DetalleInventarioFisico> DetallesInventarioFisico { get; set; } = new List<DetalleInventarioFisico>();
    }
}