using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class AplicacionPago
    {
        public int IdAplicacion { get; set; }
        public int IdEmpresa { get; set; }
        public int IdPago { get; set; }
        public int IdObligacion { get; set; }
        public decimal MontoAplicado { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Pago Pago { get; set; } = null!;
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual ObligacionCxC ObligacionCxC { get; set; } = null!;
    }
}
