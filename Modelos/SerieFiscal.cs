using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class SerieFiscal
    {
        public int IdSerieFiscal { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public string TipoComprobante { get; set; } = null!;
        public string Serie { get; set; } = null!;
        public long CorrelativoActual { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Sucursal? Sucursal { get; set; }
    }
}
