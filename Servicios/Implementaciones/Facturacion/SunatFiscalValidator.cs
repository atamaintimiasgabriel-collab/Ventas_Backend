using System.Text.Json;
using System.Xml;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion;

public sealed class SunatFiscalValidator : ISunatFiscalValidator
{
    public const string VersionNormativa = "SUNAT-CPE-2026-08-26";
    private readonly Lazy<Catalogos> _catalogos;

    public SunatFiscalValidator(IHostEnvironment environment)
        : this(Path.Combine(environment.ContentRootPath, "Resources", "Facturacion", "Sunat", "2026-08-26", "catalogs", "catalogs.json")) { }

    public SunatFiscalValidator(string catalogPath)
    {
        if (string.IsNullOrWhiteSpace(catalogPath)) throw new ArgumentException("La ruta normativa es obligatoria.", nameof(catalogPath));
        _catalogos = new Lazy<Catalogos>(() => Cargar(catalogPath), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task ValidarAsync(DocumentoElectronicoDTO documento, Stream xml, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documento);
        ArgumentNullException.ThrowIfNull(xml);
        cancellationToken.ThrowIfCancellationRequested();
        var c = _catalogos.Value;
        var errores = new List<SunatFiscalError>();

        Validar(c.DocumentTypes.Contains(documento.TipoComprobante), "SUNAT-CAT-01", "TipoComprobante", "Tipo de documento no admitido.", errores);
        Validar(documento.VersionNormativa == VersionNormativa, "SUNAT-VERSION", "VersionNormativa", "Versión normativa no soportada.", errores);
        Validar(c.Currencies.Contains(documento.Moneda), "SUNAT-CAT-02", "Moneda", "Moneda no admitida.", errores);
        Validar(EsSerieValida(documento.TipoComprobante, documento.Serie), "SUNAT-SERIE", "Serie", "Formato de serie no válido para el documento.", errores);
        Validar(documento.Correlativo is > 0 and <= 99999999, "SUNAT-CORRELATIVO", "Correlativo", "Correlativo fuera de rango.", errores);
        Validar(c.IdentityTypes.Contains(MapearIdentidad(documento.Cliente.TipoDocumentoIdentidad)), "SUNAT-CAT-06", "Cliente.TipoDocumentoIdentidad", "Identidad no admitida.", errores);

        if (documento.TipoComprobante == "01")
            Validar(MapearIdentidad(documento.Cliente.TipoDocumentoIdentidad) == "6" && documento.Cliente.NumeroDocumento.Length == 11,
                "SUNAT-01-RECEPTOR", "Cliente", "La factura requiere receptor con RUC.", errores);

        if (documento.TipoComprobante is "01" or "03")
        {
            Validar(c.OperationTypes.TryGetValue(documento.TipoOperacionSunat, out var docs) && docs.Contains(documento.TipoComprobante),
                "SUNAT-CAT-51", "TipoOperacionSunat", "Tipo de operación no aplicable.", errores);
        }

        foreach (var d in documento.Detalles)
        {
            Validar(c.Units.Contains(d.UnidadMedidaSunat), "SUNAT-CAT-03", $"Detalles[{d.Secuencia}].Unidad", "Unidad no admitida.", errores);
            Validar(c.Taxes.TryGetValue(d.CodigoTributo, out var tributo) && tributo.Name == d.NombreTributo && tributo.InternationalCode == d.CodigoInternacionalTributo,
                "SUNAT-CAT-05", $"Detalles[{d.Secuencia}].Tributo", "Tributo inconsistente.", errores);
            Validar(c.IgvAffectations.TryGetValue(d.CodigoAfectacionIgv, out var codigoTributo) && codigoTributo.Split(" o ").Contains(d.CodigoTributo),
                "SUNAT-CAT-07", $"Detalles[{d.Secuencia}].Afectacion", "Afectación y tributo inconsistentes.", errores);
            Validar(c.PriceTypes.Contains(d.TipoPrecio), "SUNAT-CAT-16", $"Detalles[{d.Secuencia}].TipoPrecio", "Tipo de precio no admitido.", errores);
            Validar(d.Cantidad > 0 && d.ValorVenta >= 0 && d.MontoImpuesto >= 0, "SUNAT-LINEA", $"Detalles[{d.Secuencia}]", "Importes o cantidad inválidos.", errores);
        }

        if (documento.TipoComprobante is "07" or "08")
        {
            var r = documento.DocumentoRelacionado;
            Validar(r is not null, "SUNAT-NOTA-REF", "DocumentoRelacionado", "La nota requiere documento afectado.", errores);
            if (r is not null)
            {
                Validar(r.TipoDocumento is "01" or "03", "SUNAT-NOTA-TIPO", "DocumentoRelacionado.TipoDocumento", "Tipo de documento afectado no admitido.", errores);
                Validar(!string.IsNullOrWhiteSpace(r.Serie) && r.Correlativo > 0, "SUNAT-NOTA-ID", "DocumentoRelacionado", "Identificación del documento afectado inválida.", errores);
                var motivos = documento.TipoComprobante == "07" ? c.CreditNoteReasons : c.DebitNoteReasons;
                Validar(motivos.Contains(r.CodigoMotivo), documento.TipoComprobante == "07" ? "SUNAT-CAT-09" : "SUNAT-CAT-10", "DocumentoRelacionado.CodigoMotivo", "Motivo no admitido.", errores);
                Validar(!string.IsNullOrWhiteSpace(r.Sustento), "SUNAT-NOTA-SUSTENTO", "DocumentoRelacionado.Sustento", "Sustento obligatorio.", errores);
            }
        }

        if (errores.Count == 0)
            await ValidarXmlAsync(documento, xml, errores, cancellationToken);

        if (errores.Count > 0) throw new SunatFiscalValidationException(errores);
    }

    private static async Task ValidarXmlAsync(DocumentoElectronicoDTO dto, Stream xml, List<SunatFiscalError> errores, CancellationToken ct)
    {
        if (xml.CanSeek) xml.Position = 0;
        var settings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 10 * 1024 * 1024 };
        using var reader = XmlReader.Create(xml, settings);
        var document = new XmlDocument { XmlResolver = null };
        document.Load(reader);
        ct.ThrowIfCancellationRequested();
        var ns = new XmlNamespaceManager(document.NameTable);
        ns.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        ns.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var root = document.DocumentElement!;
        Validar(root.SelectSingleNode("cbc:UBLVersionID", ns)?.InnerText == "2.1", "SUNAT-UBL", "UBLVersionID", "Versión UBL inválida.", errores);
        Validar(root.SelectSingleNode("cbc:CustomizationID", ns)?.InnerText == "2.0", "SUNAT-CUSTOM", "CustomizationID", "Versión de estructura inválida.", errores);
        if (dto.TipoComprobante is "01" or "03")
            Validar(root.SelectSingleNode("cbc:ProfileID", ns)?.InnerText == dto.TipoOperacionSunat, "SUNAT-PROFILE", "ProfileID", "Tipo de operación ausente o inconsistente.", errores);
        if (dto.TipoComprobante is "07" or "08")
        {
            Validar(root.SelectSingleNode("cac:DiscrepancyResponse/cbc:ReferenceID", ns) is not null, "SUNAT-NOTA-DISCREPANCY", "DiscrepancyResponse", "Referencia de discrepancia ausente.", errores);
            Validar(root.SelectSingleNode("cac:BillingReference/cac:InvoiceDocumentReference/cbc:DocumentTypeCode", ns) is not null, "SUNAT-NOTA-BILLING", "BillingReference", "Referencia fiscal ausente.", errores);
        }
        if (xml.CanSeek) xml.Position = 0;
        await Task.CompletedTask;
    }

    private static bool EsSerieValida(string tipo, string serie) =>
        tipo switch { "01" => System.Text.RegularExpressions.Regex.IsMatch(serie ?? "", "^F[A-Z0-9]{3}$"), "03" => System.Text.RegularExpressions.Regex.IsMatch(serie ?? "", "^B[A-Z0-9]{3}$"), "07" or "08" => System.Text.RegularExpressions.Regex.IsMatch(serie ?? "", "^[FB][A-Z0-9]{3}$"), _ => false };

    public static string MapearIdentidad(string value) => value?.ToUpperInvariant() switch { "RUC" => "6", "DNI" => "1", "CE" => "4", "PAS" or "PASAPORTE" => "7", "SIN_DOCUMENTO" => "0", _ => "?" };
    private static void Validar(bool ok, string code, string field, string message, List<SunatFiscalError> errors) { if (!ok) errors.Add(new(code, field, message)); }
    private static Catalogos Cargar(string path) => JsonSerializer.Deserialize<Catalogos>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("Paquete normativo inválido.");

    private sealed class Catalogos
    {
        public HashSet<string> DocumentTypes { get; set; } = [];
        public HashSet<string> Currencies { get; set; } = [];
        public HashSet<string> Units { get; set; } = [];
        public Dictionary<string, Tributo> Taxes { get; set; } = [];
        public HashSet<string> IdentityTypes { get; set; } = [];
        public Dictionary<string, string> IgvAffectations { get; set; } = [];
        public HashSet<string> CreditNoteReasons { get; set; } = [];
        public HashSet<string> DebitNoteReasons { get; set; } = [];
        public HashSet<string> PriceTypes { get; set; } = [];
        public Dictionary<string, HashSet<string>> OperationTypes { get; set; } = [];
    }
    private sealed class Tributo { public string Name { get; set; } = ""; public string InternationalCode { get; set; } = ""; }
}
