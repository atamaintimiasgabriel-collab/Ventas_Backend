namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos
{
    public class PedidoIdempotencyConflictException : PedidoDomainException
    {
        public PedidoIdempotencyConflictException(string idempotencyKey) 
            : base($"Conflicto de idempotencia detectado para la clave '{idempotencyKey}'. La solicitud actual es incompatible con la original.")
        {
        }
    }
}
