namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas
{
    public class VentaConcurrencyException : Exception
    {
        public VentaConcurrencyException(string message) : base(message)
        {
        }
    }
}
