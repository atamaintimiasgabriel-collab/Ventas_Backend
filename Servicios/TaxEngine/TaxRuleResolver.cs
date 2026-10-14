using System;
using System.Linq;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public class TaxRuleResolver : ITaxRuleResolver
    {
        public TaxRuleDTO? ResolveRule(EmpresaTaxConfigDTO config, string clasificacionTributaria, string conceptoTributario, DateTime fechaOperacion)
        {
            if (config == null || config.ReglasTributarias == null)
            {
                return null;
            }

            var reglaVigente = config.ReglasTributarias
                .Where(r => r.ClasificacionTributaria == clasificacionTributaria &&
                            r.ConceptoTributario == conceptoTributario &&
                            r.FechaInicio.Date <= fechaOperacion.Date &&
                            (!r.FechaFin.HasValue || r.FechaFin.Value.Date >= fechaOperacion.Date))
                .OrderByDescending(r => r.FechaInicio)
                .FirstOrDefault();

            return reglaVigente;
        }
    }
}
