using System;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public interface ITaxRuleResolver
    {
        TaxRuleDTO? ResolveRule(EmpresaTaxConfigDTO config, string clasificacionTributaria, string conceptoTributario, DateTime fechaOperacion);
    }
}
