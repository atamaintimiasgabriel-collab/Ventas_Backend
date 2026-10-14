namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas
{
    public class VentaIdempotencyException : Exception
    {
        public VentaIdempotencyException(string message) : base(message)
        {
        }
    }
}
