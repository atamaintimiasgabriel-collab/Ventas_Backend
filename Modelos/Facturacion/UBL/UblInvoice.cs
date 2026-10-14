using System.Xml.Serialization;

namespace SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL
{
    // ============================================================
    // INVOICE (Factura / Boleta)
    // Código de tipo: 01 = Factura, 03 = Boleta
    // ============================================================

    [XmlRoot("Invoice", Namespace = UblNamespaces.Invoice)]
    public class UblInvoice
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; set; }

        [XmlElement("UBLVersionID", Namespace = UblNamespaces.Cbc)]
        public string UBLVersionID { get; set; } = UblNamespaces.UblVersion;

        [XmlElement("CustomizationID", Namespace = UblNamespaces.Cbc)]
        public string CustomizationID { get; set; } = "2.0"; // Versión de personalización SUNAT

        [XmlElement("ProfileID", Namespace = UblNamespaces.Cbc)]
        public string ProfileID { get; set; } = null!;

        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!; // "F001-00000001"

        [XmlElement("IssueDate", Namespace = UblNamespaces.Cbc)]
        public string IssueDate { get; set; } = null!; // "2026-09-18"

        [XmlElement("IssueTime", Namespace = UblNamespaces.Cbc)]
        public string? IssueTime { get; set; } // "18:00:00"

        [XmlElement("InvoiceTypeCode", Namespace = UblNamespaces.Cbc)]
        public string InvoiceTypeCode { get; set; } = null!; // "01" o "03"

        [XmlElement("Note", Namespace = UblNamespaces.Cbc)]
        public List<string> Notes { get; set; } = new(); // Importe en letras

        [XmlElement("DocumentCurrencyCode", Namespace = UblNamespaces.Cbc)]
        public string DocumentCurrencyCode { get; set; } = "PEN";

        [XmlElement("AccountingSupplierParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingSupplierParty { get; set; } = null!;

        [XmlElement("AccountingCustomerParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingCustomerParty { get; set; } = null!;

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public List<UblTaxTotal> TaxTotals { get; set; } = new();

        [XmlElement("LegalMonetaryTotal", Namespace = UblNamespaces.Cac)]
        public UblLegalMonetaryTotal LegalMonetaryTotal { get; set; } = null!;

        [XmlElement("InvoiceLine", Namespace = UblNamespaces.Cac)]
        public List<UblInvoiceLine> InvoiceLines { get; set; } = new();

        public UblInvoice()
        {
            Namespaces = BuildNamespaces();
        }

        private static XmlSerializerNamespaces BuildNamespaces()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(UblNamespaces.PrefixCbc, UblNamespaces.Cbc);
            ns.Add(UblNamespaces.PrefixCac, UblNamespaces.Cac);
            ns.Add(UblNamespaces.PrefixExt, UblNamespaces.Ext);
            ns.Add(UblNamespaces.PrefixDs,  UblNamespaces.Ds);
            ns.Add(UblNamespaces.PrefixXsi, UblNamespaces.Xsi);
            ns.Add(string.Empty, UblNamespaces.Invoice); // namespace default
            return ns;
        }
    }
}
