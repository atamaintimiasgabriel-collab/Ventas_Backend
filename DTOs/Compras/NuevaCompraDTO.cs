using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Compras
{
    public class NuevaCompraDTO
    {
        [Required]
        public int IdProveedor { get; set; }
        public DateTime? FechaEstimada { get; set; }
        public decimal Impuesto { get; set; } // e.g. 18.00
        public string Observaciones { get; set; } = string.Empty;
        
        [Required]
        [MinLength(1)]
        public List<NuevoDetalleCompraDTO> Detalles { get; set; } = new List<NuevoDetalleCompraDTO>();
    }

    public class NuevoDetalleCompraDTO
    {
        [Required]
        public int IdProducto { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }
    }
    
    public class CambioEstadoCompraDTO
    {
        [Required]
        public string NuevoEstado { get; set; } = string.Empty; // "Recibida", "Anulada"
        public string Observaciones { get; set; } = string.Empty;
    }
}
