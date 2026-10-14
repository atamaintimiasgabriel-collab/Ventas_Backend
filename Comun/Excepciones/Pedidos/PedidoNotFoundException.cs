namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos
{
    public class PedidoNotFoundException : PedidoDomainException
    {
        public PedidoNotFoundException(int idPedido) 
            : base($"El pedido con ID {idPedido} no fue encontrado.")
        {
        }
    }
}
