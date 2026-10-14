using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class MovimientoCaja
    {
        public int IdMovCaja { get; set; }
        public int IdEmpresa { get; set; }
        public int IdApertura { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? IdOrigen { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual AperturaCaja AperturaCaja { get; set; } = null!;
        public virtual Empresa Empresa { get; set; } = null!;
    }
}
