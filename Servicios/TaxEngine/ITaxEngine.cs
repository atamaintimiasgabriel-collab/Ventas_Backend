using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public interface ITaxEngine
    {
        Task<TaxEngineResultDTO> CalculateTaxesAsync(TaxEngineRequestDTO request);
    }
}
