namespace SistemaVentasMotorrepuestos.API.DTOs.TaxEngine
{
    public class TaxCalculationResultDTO
    {
        public decimal BaseImponible { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalLinea { get; set; }
    }
}
