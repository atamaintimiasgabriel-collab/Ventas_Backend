namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    public class ClienteStatsResponse
    {
        public int Total { get; set; }
        public int Activos { get; set; }
        public int Inactivos { get; set; }
        public int NuevosDelMes { get; set; }
        public decimal TotalCompras { get; set; }
        public IReadOnlyList<ClienteGrowthPoint> Crecimiento { get; set; } = Array.Empty<ClienteGrowthPoint>();
    }
    public class ClienteGrowthPoint { public string Periodo { get; set; } = string.Empty; public int Total { get; set; } }
}
