using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class NumeradorPedido : INumeradorPedido
    {
        public string GenerarNumeroPedido(int idEmpresa, int? idSucursal)
        {
            // Estrategia temporal aprobada: PED-{ShortGuid}
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            return $"PED-{shortGuid}";
        }
    }
}
