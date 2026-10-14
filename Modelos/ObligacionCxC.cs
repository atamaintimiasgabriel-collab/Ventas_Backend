using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ObligacionCxC
    {
        public int IdObligacion { get; set; }
        public int IdEmpresa { get; set; }
        public int IdCliente { get; set; }
        public int? IdVentaOrigen { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public decimal DeudaOriginal { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Venta? Venta { get; set; }
        
        public virtual ICollection<MovimientoCxC> Movimientos { get; set; } = new HashSet<MovimientoCxC>();
        public virtual ICollection<AplicacionPago> Aplicaciones { get; set; } = new HashSet<AplicacionPago>();
    }
}
