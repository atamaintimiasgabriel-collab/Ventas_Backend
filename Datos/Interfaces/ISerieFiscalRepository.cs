using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface ISerieFiscalRepository
    {
        Task<SerieFiscal?> ObtenerPorIdAsync(int idSerieFiscal, int idEmpresa, CancellationToken cancellationToken = default);
        Task<IEnumerable<SerieFiscal>> BuscarPorEmpresaAsync(int idEmpresa, int? idSucursal = null, bool? activo = null, CancellationToken cancellationToken = default);
        Task<SerieFiscal?> BuscarSerieAsync(int idEmpresa, string tipoComprobante, string serie, CancellationToken cancellationToken = default);
        Task<SerieFiscal?> ObtenerSerieActivaAsync(int idEmpresa, int idSucursal, string tipoComprobante, CancellationToken cancellationToken = default);
        Task<bool> ExisteSerieAsync(int idEmpresa, string tipoComprobante, string serie, CancellationToken cancellationToken = default);
    }
}
