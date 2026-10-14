using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        public int IdEmpresa { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(150)]
        public string NombreComercial { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Correo { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [ForeignKey("IdEmpresa")]
        public Empresa? Empresa { get; set; }
    }
}
