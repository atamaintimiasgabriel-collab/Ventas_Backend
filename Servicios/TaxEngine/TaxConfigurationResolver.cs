using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public class TaxConfigurationResolver : ITaxConfigurationResolver
    {
        private readonly ApplicationDbContext _context;

        public TaxConfigurationResolver(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmpresaTaxConfigDTO?> GetConfigurationAsync(int idEmpresa, int? idSucursal = null)
        {
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpresa == idEmpresa && e.Activo);

            if (empresa == null || string.IsNullOrWhiteSpace(empresa.ConfiguracionFiscalJson))
            {
                return null;
            }

            try
            {
                var config = JsonSerializer.Deserialize<EmpresaTaxConfigDTO>(empresa.ConfiguracionFiscalJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config != null)
                {
                    config.IdEmpresa = idEmpresa;
                }

                return config;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
