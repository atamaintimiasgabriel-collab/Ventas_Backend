using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Servicios.TaxEngine
{
    public interface ITaxRoundingService
    {
        /// <summary>
        /// Prorratea el descuento global de forma determinística entre las líneas elegibles.
        /// Garantiza que la suma de los descuentos distribuidos sea exactamente igual al descuento global.
        /// </summary>
        /// <param name="descuentoGlobal">Monto del descuento global a repartir</param>
        /// <param name="basesElegiblesPorLinea">Un diccionario con IdLinea y la Base Elegible de cada línea</param>
        /// <returns>Diccionario con IdLinea y el Descuento Global Prorrateado asignado</returns>
        Dictionary<string, decimal> ProrratearDescuentoGlobal(decimal descuentoGlobal, Dictionary<string, decimal> basesElegiblesPorLinea);
    }
}
