using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class MovimientoCxC
    {
        public int IdMovCxC { get; set; }
        public int IdEmpresa { get; set; }
        public int IdObligacion { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? IdReferenciaOrigen { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual ObligacionCxC ObligacionCxC { get; set; } = null!;
    }
}
