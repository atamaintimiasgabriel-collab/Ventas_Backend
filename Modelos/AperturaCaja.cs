using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class AperturaCaja
    {
        public int IdApertura { get; set; }
        public int IdCaja { get; set; }
        public int IdEmpresa { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoEsperado { get; set; }
        public decimal? SaldoContado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Caja Caja { get; set; } = null!;
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
        
        public virtual ICollection<MovimientoCaja> Movimientos { get; set; } = new HashSet<MovimientoCaja>();
    }
}
