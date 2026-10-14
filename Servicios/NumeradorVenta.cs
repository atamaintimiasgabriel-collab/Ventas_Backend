using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class NumeradorVenta : INumeradorVenta
    {
        private readonly ApplicationDbContext _context;

        public NumeradorVenta(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerarNumeroVentaAsync(int idEmpresa, int? idSucursal)
        {
            var paramIdEmpresa = new SqlParameter("@IdEmpresa", idEmpresa);
            var paramTipo = new SqlParameter("@TipoDocumento", "VENTA_INTERNA");
            var outSiguiente = new SqlParameter("@SiguienteNumero", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_GenerarSiguienteCorrelativo] @IdEmpresa, @TipoDocumento, @SiguienteNumero OUT",
                paramIdEmpresa, paramTipo, outSiguiente);

            var correlativo = (long)outSiguiente.Value;
            
            // Formato estándar: V-{IdEmpresa:D4}-{Correlativo:D8}
            return $"V-{idEmpresa:D4}-{correlativo:D8}";
        }
    }
}
