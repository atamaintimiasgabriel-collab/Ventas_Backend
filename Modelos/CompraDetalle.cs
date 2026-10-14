using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    [Table("CompraDetalles")]
    public class CompraDetalle
    {
        [Key]
        public int IdCompraDetalle { get; set; }

        public int IdCompra { get; set; }
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [ForeignKey("IdCompra")]
        public Compra? Compra { get; set; }

        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }
    }
}
