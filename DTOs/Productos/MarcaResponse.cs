namespace SistemaVentasMotorrepuestos.API.DTOs.Productos
{
    /// <summary>
    /// Response DTO para marcas de productos
    /// Utilizado por filtros y listados en catálogo
    /// </summary>
    public class MarcaResponse
    {
        public int IdMarca { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}
