using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.DTOs.TaxEngine
{
    public class TaxEngineRequestDTO
    {
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public DateTime FechaOperacion { get; set; }
        public decimal DescuentoGlobal { get; set; }
        
        public List<TaxEngineLineRequestDTO> Lineas { get; set; } = new List<TaxEngineLineRequestDTO>();
    }

    public class TaxEngineLineRequestDTO
    {
        // Un identificador determinístico de la línea (puede ser un Guid o un índice)
        public string IdLinea { get; set; } = null!;
        public int IdProducto { get; set; }
        
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal DescuentoLinea { get; set; }
        
        public string ClasificacionTributaria { get; set; } = null!;
        public string ConceptoTributario { get; set; } = null!;
        
        // Indica si el PrecioUnitario ya tiene el impuesto incluido
        public bool EsPrecioBruto { get; set; }
    }
}
