using System.Xml.Serialization;

namespace SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL
{
    /// <summary>
    /// Constantes de namespaces UBL 2.1 requeridos por SUNAT/OSE.
    /// Centralizados aquí para no distribuirlos en múltiples clases.
    /// NOTA: Estos namespaces son de referencia estructural; la certificación
    /// fiscal definitiva requiere validación oficial contra XSD de SUNAT.
    /// </summary>
    public static class UblNamespaces
    {
        public const string Invoice      = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public const string CreditNote   = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
        public const string DebitNote    = "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2";

        public const string Cac          = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        public const string Cbc          = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        public const string Ext          = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        public const string Ds           = "http://www.w3.org/2000/09/xmldsig#";
        public const string Xsi          = "http://www.w3.org/2001/XMLSchema-instance";

        // Prefijos estándar
        public const string PrefixCac    = "cac";
        public const string PrefixCbc    = "cbc";
        public const string PrefixExt    = "ext";
        public const string PrefixDs     = "ds";
        public const string PrefixXsi    = "xsi";

        // Versiones UBL declaradas en los documentos raíz
        public const string UblVersion   = "2.1";
    }
}
