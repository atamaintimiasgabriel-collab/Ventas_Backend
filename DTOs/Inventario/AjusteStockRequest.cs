namespace SistemaVentasMotorrepuestos.API.DTOs.Inventario
{
    public class AjusteStockRequest
    {
        public int IdStock { get; set; }
        public decimal Cantidad { get; set; }
        public string Tipo { get; set; } = string.Empty; // SUMAR, RESTAR, REEMPLAZAR
        public string Motivo { get; set; } = string.Empty;
    }
}
