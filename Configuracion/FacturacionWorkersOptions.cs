namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    /// <summary>
    /// Controla exclusivamente el registro de los hosted workers fiscales.
    /// El valor predeterminado es false para evitar procesamiento accidental
    /// cuando la sección de configuración no existe.
    /// </summary>
    public sealed class FacturacionWorkersOptions
    {
        public const string SectionName = "Facturacion:Workers";

        public bool Enabled { get; set; }
    }
}
