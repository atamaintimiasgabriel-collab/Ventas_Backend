using System;

namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Inventario
{
    public class InventarioConcurrencyException : Exception
    {
        public InventarioConcurrencyException(string message) : base(message) { }
    }
}
