using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Sucursal Sucursal { get; set; } = null!;
        
        public virtual ICollection<AperturaCaja> AperturasCajas { get; set; } = new HashSet<AperturaCaja>();
    }
}
