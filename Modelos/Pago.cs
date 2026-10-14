using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Pago
    {
        public int IdPago { get; set; }
        public int IdEmpresa { get; set; }
        public int IdCliente { get; set; }
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public int IdMedioPago { get; set; }
        public int IdCanalPago { get; set; }
        public int? IdProveedor { get; set; }
        public string? ReferenciaExterna { get; set; }
        public string? IdempotencyKey { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? CodigoAutorizacion { get; set; }
        public string? Lote { get; set; }
        public string? Ultimos4Digitos { get; set; }
        public string? Adquirente { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual MedioPago MedioPago { get; set; } = null!;
        public virtual CanalPago CanalPago { get; set; } = null!;
        public virtual ProveedorPago? ProveedorPago { get; set; }
        
        public virtual ICollection<AplicacionPago> Aplicaciones { get; set; } = new HashSet<AplicacionPago>();
    }
}
