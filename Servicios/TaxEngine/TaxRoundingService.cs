using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public class TaxRoundingService : ITaxRoundingService
    {
        public Dictionary<string, decimal> ProrratearDescuentoGlobal(decimal descuentoGlobal, Dictionary<string, decimal> basesElegiblesPorLinea)
        {
            var resultado = new Dictionary<string, decimal>();
            if (descuentoGlobal <= 0 || basesElegiblesPorLinea == null || basesElegiblesPorLinea.Count == 0)
            {
                if (basesElegiblesPorLinea != null)
                {
                    foreach (var k in basesElegiblesPorLinea.Keys) resultado[k] = 0m;
                }
                return resultado;
            }

            decimal totalBaseElegible = basesElegiblesPorLinea.Values.Sum();
            if (totalBaseElegible <= 0)
            {
                foreach (var k in basesElegiblesPorLinea.Keys) resultado[k] = 0m;
                return resultado;
            }

            decimal sumaDescuentosAsignados = 0;
            
            // Criterio de desempate determinístico:
            // 1. Mayor base elegible (descendente)
            // 2. IdLinea (ascendente) como desempate estable
            var lineasOrdenadas = basesElegiblesPorLinea
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .ToList();

            foreach (var linea in lineasOrdenadas)
            {
                decimal peso = linea.Value / totalBaseElegible;
                decimal descuentoTeorico = descuentoGlobal * peso;
                decimal descuentoRedondeado = Math.Round(descuentoTeorico, 2, MidpointRounding.AwayFromZero);
                
                // Evitar que el descuento supere la base elegible (no bases negativas)
                if (descuentoRedondeado > linea.Value)
                {
                    descuentoRedondeado = linea.Value;
                }

                resultado[linea.Key] = descuentoRedondeado;
                sumaDescuentosAsignados += descuentoRedondeado;
            }

            decimal residuo = Math.Round(descuentoGlobal - sumaDescuentosAsignados, 2, MidpointRounding.AwayFromZero);

            if (residuo != 0 && lineasOrdenadas.Count > 0)
            {
                // Asignar el residuo a la primera línea del orden determinístico que pueda absorberlo
                // garantizando que no se genere una base negativa si el residuo es positivo (o sea falta asignar descuento)
                var lineaDestinataria = lineasOrdenadas.First().Key;
                resultado[lineaDestinataria] += residuo;
                
                // Chequeo de seguridad final por línea destinataria
                if (resultado[lineaDestinataria] > basesElegiblesPorLinea[lineaDestinataria])
                {
                     resultado[lineaDestinataria] = basesElegiblesPorLinea[lineaDestinataria];
                }
            }

            return resultado;
        }
    }
}
