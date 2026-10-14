namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface INumeradorPedido
    {
        string GenerarNumeroPedido(int idEmpresa, int? idSucursal);
    }
}
