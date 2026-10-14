using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    [Table("Compras")]
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }

        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public int IdProveedor { get; set; }
        public int IdUsuarioLog { get; set; } // The user who created this

        [Required]
        [MaxLength(50)]
        public string NumeroCompra { get; set; } = string.Empty; // e.g. "C-2024-000256"

        public DateTime FechaCompra { get; set; }
        public DateTime? FechaEstimada { get; set; }
        public DateTime? FechaRecepcion { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En tránsito, Recibida, Anulada

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuesto { get; set; } // IGV typically 18%

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [ForeignKey("IdEmpresa")]
        public Empresa? Empresa { get; set; }

        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        [ForeignKey("IdProveedor")]
        public Proveedor? Proveedor { get; set; }

        [ForeignKey("IdUsuarioLog")]
        public Usuario? Usuario { get; set; }

        public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
    }
}
