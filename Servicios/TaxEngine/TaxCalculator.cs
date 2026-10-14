using System;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public class TaxCalculator : ITaxCalculator
    {
        public TaxCalculationResultDTO CalculateLineNet(decimal precioUnitario, decimal cantidad, decimal descuentoLinea, decimal prorrateoDescuentoGlobal, decimal tasaTributaria)
        {
            decimal bruto = precioUnitario * cantidad;
            decimal baseNoRound = bruto - descuentoLinea - prorrateoDescuentoGlobal;
            
            decimal baseImponible = Math.Round(baseNoRound, 2, MidpointRounding.AwayFromZero);
            decimal impuesto = Math.Round(baseImponible * (tasaTributaria / 100m), 2, MidpointRounding.AwayFromZero);
            decimal totalLinea = baseImponible + impuesto;

            return new TaxCalculationResultDTO
            {
                BaseImponible = baseImponible,
                MontoImpuesto = impuesto,
                TotalLinea = totalLinea
            };
        }

        public TaxCalculationResultDTO CalculateLineGross(decimal precioBruto, decimal descuentoLinea, decimal prorrateoDescuentoGlobal, decimal tasaTributaria)
        {
            // precioBruto here is equivalent to (PrecioUnitarioBruto * Cantidad)
            decimal importeBrutoFinal = precioBruto - descuentoLinea - prorrateoDescuentoGlobal;
            
            decimal baseNoRound = importeBrutoFinal / (1m + (tasaTributaria / 100m));
            decimal baseImponible = Math.Round(baseNoRound, 2, MidpointRounding.AwayFromZero);
            decimal impuesto = importeBrutoFinal - baseImponible;
            
            decimal totalLinea = baseImponible + impuesto;

            return new TaxCalculationResultDTO
            {
                BaseImponible = baseImponible,
                MontoImpuesto = impuesto,
                TotalLinea = totalLinea
            };
        }
    }
}
