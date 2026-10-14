using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ProveedorPago
    {
        public int IdProveedor { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public virtual Empresa Empresa { get; set; } = null!;
    }
}
