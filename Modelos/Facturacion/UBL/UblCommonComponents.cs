using System.Xml;
using System.Xml.Serialization;

namespace SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL
{
    // ============================================================
    // COMPONENTES COMUNES (cbc / cac)
    // Clases reutilizables entre Invoice, CreditNote, DebitNote
    // ============================================================

    /// <summary>Importe con código de moneda (cbc:TaxableAmount, etc.)</summary>
    public class UblAmount
    {
        [XmlAttribute("currencyID")]
        public string CurrencyID { get; set; } = "PEN";

        [XmlText]
        public decimal Value { get; set; }
    }

    /// <summary>cbc:ID — identificador simple de texto</summary>
    public class UblId
    {
        [XmlText]
        public string Value { get; set; } = null!;
    }

    /// <summary>cac:TaxScheme — esquema tributario (IGV, ISC, etc.)</summary>
    public class UblTaxScheme
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("Name", Namespace = UblNamespaces.Cbc)]
        public string Name { get; set; } = null!;

        [XmlElement("TaxTypeCode", Namespace = UblNamespaces.Cbc)]
        public string TaxTypeCode { get; set; } = null!;
    }

    /// <summary>cac:TaxCategory — categoría de impuesto por línea</summary>
    public class UblTaxCategory
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("Percent", Namespace = UblNamespaces.Cbc)]
        public string Percent { get; set; } = null!;   // "18.00" / "0.00"

        [XmlElement("TaxExemptionReasonCode", Namespace = UblNamespaces.Cbc)]
        public string? TaxExemptionReasonCode { get; set; }

        [XmlElement("TaxScheme", Namespace = UblNamespaces.Cac)]
        public UblTaxScheme TaxScheme { get; set; } = null!;
    }

    /// <summary>cac:TaxSubtotal — subtotal de impuesto a nivel de documento</summary>
    public class UblTaxSubtotal
    {
        [XmlElement("TaxableAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount TaxableAmount { get; set; } = null!;

        [XmlElement("TaxAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount TaxAmount { get; set; } = null!;

        [XmlElement("TaxCategory", Namespace = UblNamespaces.Cac)]
        public UblTaxCategory TaxCategory { get; set; } = null!;
    }

    /// <summary>cac:TaxTotal — total de impuestos</summary>
    public class UblTaxTotal
    {
        [XmlElement("TaxAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount TaxAmount { get; set; } = null!;

        [XmlElement("TaxSubtotal", Namespace = UblNamespaces.Cac)]
        public List<UblTaxSubtotal> TaxSubtotals { get; set; } = new();
    }

    /// <summary>cac:Party — entidad (empresa emisora o cliente)</summary>
    public class UblParty
    {
        [XmlElement("PartyIdentification", Namespace = UblNamespaces.Cac)]
        public UblPartyIdentification PartyIdentification { get; set; } = null!;

        [XmlElement("PartyName", Namespace = UblNamespaces.Cac)]
        public UblPartyName? PartyName { get; set; }

        [XmlElement("PartyLegalEntity", Namespace = UblNamespaces.Cac)]
        public UblPartyLegalEntity PartyLegalEntity { get; set; } = null!;

        [XmlElement("Contact", Namespace = UblNamespaces.Cac)]
        public UblContact? Contact { get; set; }
    }

    public class UblPartyIdentification
    {
        // XmlAnyElement evita que XmlSerializer confunda este cbc:ID con los
        // cbc:ID simples, manteniendo el atributo schemeID requerido.
        [XmlIgnore]
        public UblIdWithScheme ID { get; set; } = null!;

        [XmlAnyElement]
        public XmlElement SerializedID
        {
            get
            {
                var document = new XmlDocument();
                var element = document.CreateElement(UblNamespaces.PrefixCbc, "ID", UblNamespaces.Cbc);
                element.SetAttribute("schemeID", ID.SchemeID);
                element.InnerText = ID.Value;
                return element;
            }
            set
            {
                ID = new UblIdWithScheme
                {
                    SchemeID = value.GetAttribute("schemeID"),
                    Value = value.InnerText
                };
            }
        }
    }

    /// <summary>
    /// Identificador con atributo schemeID. Usa [XmlText]+[XmlAttribute].
    /// IMPORTANTE: Este patrón solo funciona con XmlSerializer cuando el elemento padre
    /// NO declara un namespace diferente en [XmlElement] (se hereda del contexto).
    /// </summary>
    public class UblIdWithScheme
    {
        [XmlAttribute("schemeID")]
        public string SchemeID { get; set; } = null!;  // "6"=RUC, "1"=DNI, etc.

        [XmlText]
        public string Value { get; set; } = null!;
    }

    public class UblPartyName
    {
        [XmlElement("Name", Namespace = UblNamespaces.Cbc)]
        public string Name { get; set; } = null!;
    }

    public class UblPartyLegalEntity
    {
        [XmlElement("RegistrationName", Namespace = UblNamespaces.Cbc)]
        public string RegistrationName { get; set; } = null!;

        [XmlElement("RegistrationAddress", Namespace = UblNamespaces.Cac)]
        public UblRegistrationAddress? RegistrationAddress { get; set; }
    }

    public class UblRegistrationAddress
    {
        [XmlElement("AddressLine", Namespace = UblNamespaces.Cac)]
        public UblAddressLine? AddressLine { get; set; }
    }

    public class UblAddressLine
    {
        [XmlElement("Line", Namespace = UblNamespaces.Cbc)]
        public string Line { get; set; } = null!;
    }

    public class UblContact
    {
        [XmlElement("ElectronicMail", Namespace = UblNamespaces.Cbc)]
        public string? ElectronicMail { get; set; }
    }

    /// <summary>cac:AccountingSupplierParty / cac:AccountingCustomerParty</summary>
    public class UblAccountingParty
    {
        [XmlElement("Party", Namespace = UblNamespaces.Cac)]
        public UblParty Party { get; set; } = null!;
    }

    /// <summary>cac:LegalMonetaryTotal — totales monetarios del documento</summary>
    public class UblLegalMonetaryTotal
    {
        [XmlElement("LineExtensionAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount LineExtensionAmount { get; set; } = null!;

        [XmlElement("TaxExclusiveAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount TaxExclusiveAmount { get; set; } = null!;

        [XmlElement("TaxInclusiveAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount TaxInclusiveAmount { get; set; } = null!;

        [XmlElement("AllowanceTotalAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount? AllowanceTotalAmount { get; set; }

        [XmlElement("PayableAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount PayableAmount { get; set; } = null!;
    }

    /// <summary>
    /// Línea de Factura (InvoicedQuantity)
    /// </summary>
    public class UblInvoiceLine
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("InvoicedQuantity", Namespace = UblNamespaces.Cbc)]
        public UblQuantity Quantity { get; set; } = null!;

        [XmlElement("LineExtensionAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount LineExtensionAmount { get; set; } = null!;

        [XmlElement("PricingReference", Namespace = UblNamespaces.Cac)]
        public UblPricingReference? PricingReference { get; set; }

        [XmlElement("AllowanceCharge", Namespace = UblNamespaces.Cac)]
        public UblAllowanceCharge? AllowanceCharge { get; set; }

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public UblTaxTotal TaxTotal { get; set; } = null!;

        [XmlElement("Item", Namespace = UblNamespaces.Cac)]
        public UblItem Item { get; set; } = null!;

        [XmlElement("Price", Namespace = UblNamespaces.Cac)]
        public UblPrice Price { get; set; } = null!;
    }

    /// <summary>
    /// Línea de Nota de Crédito (CreditedQuantity)
    /// </summary>
    public class UblCreditNoteLine
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("CreditedQuantity", Namespace = UblNamespaces.Cbc)]
        public UblQuantity Quantity { get; set; } = null!;

        [XmlElement("LineExtensionAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount LineExtensionAmount { get; set; } = null!;

        [XmlElement("PricingReference", Namespace = UblNamespaces.Cac)]
        public UblPricingReference? PricingReference { get; set; }

        [XmlElement("AllowanceCharge", Namespace = UblNamespaces.Cac)]
        public UblAllowanceCharge? AllowanceCharge { get; set; }

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public UblTaxTotal TaxTotal { get; set; } = null!;

        [XmlElement("Item", Namespace = UblNamespaces.Cac)]
        public UblItem Item { get; set; } = null!;

        [XmlElement("Price", Namespace = UblNamespaces.Cac)]
        public UblPrice Price { get; set; } = null!;
    }

    /// <summary>
    /// Línea de Nota de Débito (DebitedQuantity)
    /// </summary>
    public class UblDebitNoteLine
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("DebitedQuantity", Namespace = UblNamespaces.Cbc)]
        public UblQuantity Quantity { get; set; } = null!;

        [XmlElement("LineExtensionAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount LineExtensionAmount { get; set; } = null!;

        [XmlElement("PricingReference", Namespace = UblNamespaces.Cac)]
        public UblPricingReference? PricingReference { get; set; }

        [XmlElement("AllowanceCharge", Namespace = UblNamespaces.Cac)]
        public UblAllowanceCharge? AllowanceCharge { get; set; }

        [XmlElement("TaxTotal", Namespace = UblNamespaces.Cac)]
        public UblTaxTotal TaxTotal { get; set; } = null!;

        [XmlElement("Item", Namespace = UblNamespaces.Cac)]
        public UblItem Item { get; set; } = null!;

        [XmlElement("Price", Namespace = UblNamespaces.Cac)]
        public UblPrice Price { get; set; } = null!;
    }

    public class UblQuantity
    {
        [XmlAttribute("unitCode")]
        public string UnitCode { get; set; } = "NIU";  // NIU = Unidad

        [XmlText]
        public decimal Value { get; set; }
    }

    public class UblPricingReference
    {
        [XmlElement("AlternativeConditionPrice", Namespace = UblNamespaces.Cac)]
        public UblAlternativeConditionPrice AlternativeConditionPrice { get; set; } = null!;
    }

    public class UblAlternativeConditionPrice
    {
        [XmlElement("PriceAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount PriceAmount { get; set; } = null!;

        [XmlElement("PriceTypeCode", Namespace = UblNamespaces.Cbc)]
        public string PriceTypeCode { get; set; } = null!; // "01"=precio venta unitario
    }

    public class UblAllowanceCharge
    {
        [XmlElement("ChargeIndicator", Namespace = UblNamespaces.Cbc)]
        public bool ChargeIndicator { get; set; }  // false = descuento

        [XmlElement("Amount", Namespace = UblNamespaces.Cbc)]
        public UblAmount Amount { get; set; } = null!;
    }

    public class UblItem
    {
        [XmlElement("Description", Namespace = UblNamespaces.Cbc)]
        public string Description { get; set; } = null!;

        [XmlElement("SellersItemIdentification", Namespace = UblNamespaces.Cac)]
        public UblSellersItemIdentification? SellersItemIdentification { get; set; }
    }

    public class UblSellersItemIdentification
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;
    }

    public class UblPrice
    {
        [XmlElement("PriceAmount", Namespace = UblNamespaces.Cbc)]
        public UblAmount PriceAmount { get; set; } = null!;
    }

    /// <summary>Referencia a documento de origen (para NC y ND)</summary>
    public class UblBillingReference
    {
        [XmlElement("InvoiceDocumentReference", Namespace = UblNamespaces.Cac)]
        public UblDocumentReference InvoiceDocumentReference { get; set; } = null!;
    }

    public class UblDocumentReference
    {
        [XmlElement("ID", Namespace = UblNamespaces.Cbc)]
        public string ID { get; set; } = null!;

        [XmlElement("DocumentTypeCode", Namespace = UblNamespaces.Cbc)]
        public string? DocumentTypeCode { get; set; }
    }
}
