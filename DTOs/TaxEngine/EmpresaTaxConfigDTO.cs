using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.DTOs.TaxEngine
{
    public class EmpresaTaxConfigDTO
    {
        public int IdEmpresa { get; set; }
        public bool PreciosIncluyenImpuestos { get; set; } = true;
        public List<TaxRuleDTO> ReglasTributarias { get; set; } = new List<TaxRuleDTO>();
    }

    public class TaxRuleDTO
    {
        public string ClasificacionTributaria { get; set; } = null!;
        public string ConceptoTributario { get; set; } = null!;
        public decimal TasaTributaria { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
