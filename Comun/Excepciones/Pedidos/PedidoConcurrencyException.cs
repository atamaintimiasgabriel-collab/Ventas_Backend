namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos
{
    public class PedidoConcurrencyException : PedidoDomainException
    {
        public PedidoConcurrencyException(string message) : base(message)
        {
        }
    }
}
