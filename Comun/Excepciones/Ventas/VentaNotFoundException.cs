namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas
{
    public class VentaNotFoundException : Exception
    {
        public VentaNotFoundException(int idVenta) 
            : base($"La venta con ID {idVenta} no fue encontrada o no pertenece a la empresa actual.")
        {
        }
    }
}
