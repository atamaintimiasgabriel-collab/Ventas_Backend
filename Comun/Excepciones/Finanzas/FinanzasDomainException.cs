namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;

public class FinanzasDomainException : Exception
{
    public FinanzasDomainException(string message) : base(message) { }
}
