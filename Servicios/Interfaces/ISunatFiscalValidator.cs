using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

public interface ISunatFiscalValidator
{
    Task ValidarAsync(DocumentoElectronicoDTO documento, Stream xml, CancellationToken cancellationToken = default);
}

public sealed record SunatFiscalError(string Codigo, string Campo, string Mensaje);

public sealed class SunatFiscalValidationException : Exception
{
    public IReadOnlyList<SunatFiscalError> Errores { get; }

    public SunatFiscalValidationException(IEnumerable<SunatFiscalError> errores)
        : base("El documento no cumple la conformidad fiscal SUNAT pre-firma.")
    {
        Errores = errores.ToArray();
    }
}
