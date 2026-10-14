using System;

namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Inventario
{
    public class InventarioDomainException : Exception
    {
        public InventarioDomainException(string message) : base(message) { }
    }
}
