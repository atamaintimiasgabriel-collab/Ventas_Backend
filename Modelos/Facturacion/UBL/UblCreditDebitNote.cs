using System.Xml.Serialization;

namespace SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL
{
    // ============================================================
    // CREDIT NOTE (Nota de Crédito)
    // Código de tipo: 07
    // ============================================================

    [XmlRoot("CreditNote", Namespace = UblNamespaces.CreditNote)]
    public class UblCreditNote
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; set; }

        [XmlElement("UBLVersionID", Namespace = UblNamespaces.Cbc)]
        public string UBLVersionID { get; set; } = UblNamespaces.UblVersion;

        [XmlElement("CustomizationID", Namespace = UblNamespaces.Cbc)]
        public string CustomizationID { get; set; } = "2.0";

        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!; // "FC01-00000001"

        [XmlElement("IssueDate", Namespace = UblNamespaces.Cbc)]
        public string IssueDate { get; set; } = null!;

        [XmlElement("IssueTime", Namespace = UblNamespaces.Cbc)]
        public string? IssueTime { get; set; }

        [XmlElement("Note", Namespace = UblNamespaces.Cbc)]
        public List<string> Notes { get; set; } = new();

        [XmlElement("DocumentCurrencyCode", Namespace = UblNamespaces.Cbc)]
        public string DocumentCurrencyCode { get; set; } = "PEN";

        [XmlElement("DiscrepancyResponse", Namespace = UblNamespaces.Cac)]
        public UblDiscrepancyResponse? DiscrepancyResponse { get; set; }

        [XmlElement("BillingReference", Namespace = UblNamespaces.Cac)]
        public UblBillingReference? BillingReference { get; set; }

        [XmlElement("AccountingSupplierParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingSupplierParty { get; set; } = null!;

        [XmlElement("AccountingCustomerParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingCustomerParty { get; set; } = null!;

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public List<UblTaxTotal> TaxTotals { get; set; } = new();

        [XmlElement("LegalMonetaryTotal", Namespace = UblNamespaces.Cac)]
        public UblLegalMonetaryTotal LegalMonetaryTotal { get; set; } = null!;

        [XmlElement("CreditNoteLine", Namespace = UblNamespaces.Cac)]
        public List<UblCreditNoteLine> CreditNoteLines { get; set; } = new();

        public UblCreditNote()
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
            ns.Add(string.Empty, UblNamespaces.CreditNote);
            return ns;
        }
    }

    // ============================================================
    // DEBIT NOTE (Nota de Débito)
    // Código de tipo: 08
    // ============================================================

    [XmlRoot("DebitNote", Namespace = UblNamespaces.DebitNote)]
    public class UblDebitNote
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; set; }

        [XmlElement("UBLVersionID", Namespace = UblNamespaces.Cbc)]
        public string UBLVersionID { get; set; } = UblNamespaces.UblVersion;

        [XmlElement("CustomizationID", Namespace = UblNamespaces.Cbc)]
        public string CustomizationID { get; set; } = "2.0";

        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!; // "FD01-00000001"

        [XmlElement("IssueDate", Namespace = UblNamespaces.Cbc)]
        public string IssueDate { get; set; } = null!;

        [XmlElement("IssueTime", Namespace = UblNamespaces.Cbc)]
        public string? IssueTime { get; set; }

        [XmlElement("Note", Namespace = UblNamespaces.Cbc)]
        public List<string> Notes { get; set; } = new();

        [XmlElement("DocumentCurrencyCode", Namespace = UblNamespaces.Cbc)]
        public string DocumentCurrencyCode { get; set; } = "PEN";

        [XmlElement("DiscrepancyResponse", Namespace = UblNamespaces.Cac)]
        public UblDiscrepancyResponse? DiscrepancyResponse { get; set; }

        [XmlElement("BillingReference", Namespace = UblNamespaces.Cac)]
        public UblBillingReference? BillingReference { get; set; }

        [XmlElement("AccountingSupplierParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingSupplierParty { get; set; } = null!;

        [XmlElement("AccountingCustomerParty", Namespace = UblNamespaces.Cac)]
        public UblAccountingParty AccountingCustomerParty { get; set; } = null!;

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public List<UblTaxTotal> TaxTotals { get; set; } = new();

        [XmlElement("RequestedMonetaryTotal", Namespace = UblNamespaces.Cac)]
        public UblLegalMonetaryTotal RequestedMonetaryTotal { get; set; } = null!;

        [XmlElement("DebitNoteLine", Namespace = UblNamespaces.Cac)]
        public List<UblDebitNoteLine> DebitNoteLines { get; set; } = new();

        public UblDebitNote()
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
            ns.Add(string.Empty, UblNamespaces.DebitNote);
            return ns;
        }
    }

    /// <summary>cac:DiscrepancyResponse — motivo de la nota de crédito o débito</summary>
    public class UblDiscrepancyResponse
    {
        [XmlElement("ReferenceID", Namespace = UblNamespaces.Cbc)]
        public string ReferenceID { get; set; } = null!;

        [XmlElement("ResponseCode", Namespace = UblNamespaces.Cbc)]
        public string ResponseCode { get; set; } = null!; // "01" = anulación, etc.

        [XmlElement("Description", Namespace = UblNamespaces.Cbc)]
        public string Description { get; set; } = null!;
    }
}
