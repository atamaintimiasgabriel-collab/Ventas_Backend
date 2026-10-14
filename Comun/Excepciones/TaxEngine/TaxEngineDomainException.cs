using System;

namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.TaxEngine
{
    public class TaxEngineDomainException : Exception
    {
        public TaxEngineDomainException(string message) : base(message)
        {
        }

        public TaxEngineDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
