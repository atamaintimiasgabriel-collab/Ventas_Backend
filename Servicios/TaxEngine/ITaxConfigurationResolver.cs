using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public interface ITaxConfigurationResolver
    {
        Task<EmpresaTaxConfigDTO?> GetConfigurationAsync(int idEmpresa, int? idSucursal = null);
    }
}
