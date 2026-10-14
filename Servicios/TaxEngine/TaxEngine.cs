using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public class TaxEngine : ITaxEngine
    {
        private readonly ITaxConfigurationResolver _configResolver;
        private readonly ITaxRuleResolver _ruleResolver;
        private readonly ITaxRoundingService _roundingService;
        private readonly ITaxCalculator _taxCalculator;

        public TaxEngine(
            ITaxConfigurationResolver configResolver,
            ITaxRuleResolver ruleResolver,
            ITaxRoundingService roundingService,
            ITaxCalculator taxCalculator)
        {
            _configResolver = configResolver;
            _ruleResolver = ruleResolver;
            _roundingService = roundingService;
            _taxCalculator = taxCalculator;
        }

        public async Task<TaxEngineResultDTO> CalculateTaxesAsync(TaxEngineRequestDTO request)
        {
            var result = new TaxEngineResultDTO();

            // 1. Resolver configuración empresarial
            var empresaConfig = await _configResolver.GetConfigurationAsync(request.IdEmpresa, request.IdSucursal);
            if (empresaConfig == null)
            {
                throw new SistemaVentasMotorrepuestos.API.Comun.Excepciones.TaxEngine.TaxEngineDomainException($"No se encontró configuración tributaria para la empresa {request.IdEmpresa}");
            }

            // 2. Prorratear descuento global
            var basesElegibles = request.Lineas.ToDictionary(
                l => l.IdLinea, 
                l => (l.PrecioUnitario * l.Cantidad) - l.DescuentoLinea
            );
            var descuentosAsignados = _roundingService.ProrratearDescuentoGlobal(request.DescuentoGlobal, basesElegibles);

            // 3. Procesar cada línea
            foreach (var reqLinea in request.Lineas)
            {
                // Resolver Regla Tributaria
                var regla = _ruleResolver.ResolveRule(
                    empresaConfig, 
                    reqLinea.ClasificacionTributaria, 
                    reqLinea.ConceptoTributario, 
                    request.FechaOperacion);

                if (regla == null)
                {
                    throw new SistemaVentasMotorrepuestos.API.Comun.Excepciones.TaxEngine.TaxEngineDomainException($"No se encontró una regla tributaria vigente para {reqLinea.ClasificacionTributaria} / {reqLinea.ConceptoTributario} en la fecha {request.FechaOperacion}");
                }

                decimal tasaAplicada = regla.TasaTributaria;
                
                decimal dctoGlobalLinea = descuentosAsignados.ContainsKey(reqLinea.IdLinea) 
                    ? descuentosAsignados[reqLinea.IdLinea] 
                    : 0m;

                TaxCalculationResultDTO calcResult;

                // Override EsPrecioBruto based on Empresa configuration
                bool isGross = empresaConfig.PreciosIncluyenImpuestos;

                if (isGross)
                {
                    calcResult = _taxCalculator.CalculateLineGross(
                        reqLinea.PrecioUnitario * reqLinea.Cantidad, 
                        reqLinea.DescuentoLinea, 
                        dctoGlobalLinea, 
                        tasaAplicada);
                }
                else
                {
                    calcResult = _taxCalculator.CalculateLineNet(
                        reqLinea.PrecioUnitario, 
                        reqLinea.Cantidad, 
                        reqLinea.DescuentoLinea, 
                        dctoGlobalLinea, 
                        tasaAplicada);
                }

                var resLinea = new TaxEngineLineResultDTO
                {
                    IdLinea = reqLinea.IdLinea,
                    IdProducto = reqLinea.IdProducto,
                    PrecioUnitario = reqLinea.PrecioUnitario,
                    Cantidad = reqLinea.Cantidad,
                    DescuentoLinea = reqLinea.DescuentoLinea,
                    DescuentoGlobalProrrateado = dctoGlobalLinea,
                    ClasificacionTributariaAplicada = reqLinea.ClasificacionTributaria,
                    ConceptoTributarioAplicado = reqLinea.ConceptoTributario,
                    TasaAplicada = tasaAplicada,
                    BaseImponible = calcResult.BaseImponible,
                    MontoImpuesto = calcResult.MontoImpuesto,
                    TotalLinea = calcResult.TotalLinea
                };

                result.Lineas.Add(resLinea);
            }

            // 4. Sumar totales
            result.TotalDescuentoGlobal = result.Lineas.Sum(l => l.DescuentoGlobalProrrateado);
            result.TotalBaseImponible = result.Lineas.Sum(l => l.BaseImponible);
            result.TotalImpuestos = result.Lineas.Sum(l => l.MontoImpuesto);
            result.TotalGeneral = result.Lineas.Sum(l => l.TotalLinea);

            return result;
        }
    }
}
