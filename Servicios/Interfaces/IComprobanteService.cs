using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IComprobanteService
    {
        Task<Comprobante> CrearComprobanteAsync(int idEmpresa, int idVenta, string tipoComprobante, string serieFiscal, string usuarioCreacion, string idempotencyKey, CancellationToken cancellationToken = default);
        Task<Comprobante?> ConsultarEstadoAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default);
        Task<IEnumerable<ComprobanteAuditoria>> ConsultarHistorialAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default);
        Task SolicitarProcesamientoAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default);
    }
}
