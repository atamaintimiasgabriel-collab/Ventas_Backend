using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.DTOs.Compras
{
    public class CompraDTO
    {
        public int IdCompra { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public DateTime? FechaEstimada { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        
        public ProveedorDTO Proveedor { get; set; } = new ProveedorDTO();
        public List<CompraDetalleDTO> Detalles { get; set; } = new List<CompraDetalleDTO>();
    }

    public class CompraDetalleDTO
    {
        public int IdCompraDetalle { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
