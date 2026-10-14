using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Pedidos;
using SistemaVentasMotorrepuestos.API.DTOs.Pedidos;

namespace SistemaVentasMotorrepuestos.API.Validadores.Pedidos
{
    public static class CrearPedidoRequestValidator
    {
        public static void Validar(CrearPedidoRequest request)
        {
            if (request == null)
                throw new PedidoDomainException("El request no puede ser nulo.");

            if (request.IdCliente <= 0)
                throw new PedidoDomainException("ID de cliente inválido.");

            if (request.Productos == null || request.Productos.Count == 0)
                throw new PedidoDomainException("El pedido debe contener al menos un producto.");

            foreach (var detalle in request.Productos)
            {
                CrearPedidoDetalleRequestValidator.Validar(detalle);
            }
        }
    }
}
