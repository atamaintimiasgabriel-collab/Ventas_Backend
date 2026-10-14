namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas
{
    public class VentaDomainException : Exception
    {
        public VentaDomainException(string message) : base(message)
        {
        }
    }
}
