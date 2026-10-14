namespace SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;

public class FinanzasConcurrencyException : Exception
{
    public FinanzasConcurrencyException(string message) : base(message) { }
}
