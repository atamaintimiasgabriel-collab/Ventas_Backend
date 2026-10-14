namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface INumeradorVenta
    {
        Task<string> GenerarNumeroVentaAsync(int idEmpresa, int? idSucursal);
    }
}
