using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Pedidos
{
    public class CrearPedidoRequest
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de cliente inválido.")]
        public int IdCliente { get; set; }

        public int? IdSucursal { get; set; }
        
        public int? IdDireccionEntrega { get; set; }

        [StringLength(20, ErrorMessage = "El origen no puede exceder 20 caracteres.")]
        public string? Origen { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        public string? Observaciones { get; set; }

        [StringLength(50, ErrorMessage = "La clave de idempotencia no puede exceder los 50 caracteres.")]
        public string? IdempotencyKey { get; set; }

        public decimal DescuentoGlobal { get; set; } = 0m;

        [Required(ErrorMessage = "La lista de productos es obligatoria.")]
        public List<CrearPedidoDetalleRequest> Productos { get; set; } = new();
    }
}
