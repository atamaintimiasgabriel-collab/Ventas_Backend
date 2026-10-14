using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Ventas
{
    public class AnularVentaRequest
    {
        [Required(ErrorMessage = "El motivo de la anulación es obligatorio.")]
        [StringLength(255, ErrorMessage = "El motivo no puede exceder los 255 caracteres.")]
        public string Motivo { get; set; } = null!;
    }
}
