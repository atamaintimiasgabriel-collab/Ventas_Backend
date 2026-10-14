using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Pedidos
{
    public class CrearPedidoDetalleRequest
    {
        [Required(ErrorMessage = "El producto es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de producto inválido.")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(0.0001, 10000.0, ErrorMessage = "La cantidad debe ser mayor a 0 y razonable.")]
        public decimal Cantidad { get; set; }
    }
}
