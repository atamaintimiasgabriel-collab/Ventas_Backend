using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion
{
    public interface IPdfGenerator
    {
        Task<byte[]> GenerarPdfAsync(DocumentoElectronicoDTO documento, ConfiguracionPdfDTO configuracion);
    }
}
