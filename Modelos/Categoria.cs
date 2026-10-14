namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public ICollection<Subcategoria> Subcategorias { get; set; } = new List<Subcategoria>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}