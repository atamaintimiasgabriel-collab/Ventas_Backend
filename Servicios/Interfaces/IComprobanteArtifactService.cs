using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IComprobanteArtifactService
    {
        Task<string> AsegurarXmlAsync(
            DocumentoElectronicoDTO documento,
            string? storageKeyXmlPersistida,
            CancellationToken cancellationToken = default);
    }
}
