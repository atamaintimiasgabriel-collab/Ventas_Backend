using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    public class ActualizarClienteRequest
    {
        [Required, StringLength(200)] public string? RazonSocial { get; set; }
        [StringLength(200)] public string? NombreComercial { get; set; }
        [StringLength(100)] public string? Nombres { get; set; }
        [StringLength(100)] public string? Apellidos { get; set; }
        [EmailAddress, StringLength(100)] public string? Email { get; set; }
        [StringLength(20)] public string? Telefono { get; set; }
    }
}
