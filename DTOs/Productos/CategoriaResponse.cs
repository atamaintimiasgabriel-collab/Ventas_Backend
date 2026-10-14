namespace SistemaVentasMotorrepuestos.API.DTOs.Productos
{
    /// <summary>
    /// Response DTO para categorías de productos
    /// Utilizado por filtros y listados en catálogo
    /// </summary>
    public class CategoriaResponse
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}
