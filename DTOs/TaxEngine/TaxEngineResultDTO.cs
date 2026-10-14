using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.DTOs.TaxEngine
{
    public class TaxEngineResultDTO
    {
        public List<TaxEngineLineResultDTO> Lineas { get; set; } = new List<TaxEngineLineResultDTO>();
        public decimal TotalDescuentoGlobal { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalBaseImponible { get; set; }
        public decimal TotalGeneral { get; set; }
    }

    public class TaxEngineLineResultDTO
    {
        public string IdLinea { get; set; } = null!;
        public int IdProducto { get; set; }
        
        // Snapshot original
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal DescuentoLinea { get; set; }
        public decimal DescuentoGlobalProrrateado { get; set; }
        
        // Snapshot tributario aplicable
        public string ClasificacionTributariaAplicada { get; set; } = null!;
        public string ConceptoTributarioAplicado { get; set; } = null!;
        public decimal TasaAplicada { get; set; }
        
        // Resultado matemático a 2 decimales
        public decimal BaseImponible { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
