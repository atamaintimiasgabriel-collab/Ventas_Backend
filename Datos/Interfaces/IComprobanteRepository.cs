using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IComprobanteRepository : IRepositorioBase<Comprobante>
    {
        Task<Comprobante?> ObtenerPorIdAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default);
        Task<Comprobante?> ObtenerPorIdConAuditoriaAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default);
        Task<IEnumerable<Comprobante>> ObtenerPorVentaAsync(int idVenta, int idEmpresa, CancellationToken cancellationToken = default);
        Task<Comprobante?> ObtenerPorIdentidadFiscalAsync(int idEmpresa, string tipoComprobante, string serieFiscal, long correlativoFiscal, CancellationToken cancellationToken = default);
        Task<IEnumerable<Comprobante>> ObtenerPorIdComprobanteOrigenAsync(Guid idComprobanteOrigen, int idEmpresa, CancellationToken cancellationToken = default);
        Task<IEnumerable<Comprobante>> ObtenerPendientesEmisionAsync(int idEmpresa, int limit, CancellationToken cancellationToken = default);
        Task<IEnumerable<Comprobante>> ObtenerPendientesConsultaAsync(int idEmpresa, int limit, CancellationToken cancellationToken = default);
        Task<bool> ExisteComprobanteParaVentaAsync(int idVenta, int idEmpresa, CancellationToken cancellationToken = default);
        Task<bool> ExisteIdentidadFiscalAsync(int idEmpresa, string tipoComprobante, string serieFiscal, long correlativoFiscal, CancellationToken cancellationToken = default);
    }
}
