using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;
using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;

namespace SistemaVentasMotorrepuestos.API.Validadores.Pedidos
{
    public static class CrearPedidoDetalleRequestValidator
    {
        public static void Validar(CrearPedidoDetalleRequest request)
        {
            if (request == null)
                throw new PedidoDomainException("El detalle del pedido no puede ser nulo.");

            if (request.IdProducto <= 0)
                throw new PedidoDomainException("ID de producto inválido.");

            if (request.Cantidad <= 0)
                throw new PedidoDomainException("La cantidad debe ser mayor a 0.");

            if (request.Cantidad > 10000)
                throw new PedidoDomainException("La cantidad excede el límite permitido.");
        }
    }
}
