using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public interface ITaxCalculator
    {
        TaxCalculationResultDTO CalculateLineNet(decimal precioUnitario, decimal cantidad, decimal descuentoLinea, decimal prorrateoDescuentoGlobal, decimal tasaTributaria);
        TaxCalculationResultDTO CalculateLineGross(decimal precioBruto, decimal descuentoLinea, decimal prorrateoDescuentoGlobal, decimal tasaTributaria);
    }
}
