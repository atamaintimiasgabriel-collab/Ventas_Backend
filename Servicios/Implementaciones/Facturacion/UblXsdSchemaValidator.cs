using System.Net;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Extensions.Hosting;
using SistemaVentasMotorrepuestos.API.Modelos.Facturacion.UBL;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    public sealed class UblXsdSchemaValidator : IXmlSchemaValidator
    {
        private const long MaxXmlCharacters = 10 * 1024 * 1024;
        private readonly string _schemaRoot;
        private readonly Lazy<XmlSchemaSet> _schemas;

        private static readonly IReadOnlyDictionary<string, (string Root, string Namespace, string File)> SchemaByType =
            new Dictionary<string, (string, string, string)>(StringComparer.Ordinal)
            {
                ["01"] = ("Invoice", UblNamespaces.Invoice, "UBL-Invoice-2.1.xsd"),
                ["03"] = ("Invoice", UblNamespaces.Invoice, "UBL-Invoice-2.1.xsd"),
                ["07"] = ("CreditNote", UblNamespaces.CreditNote, "UBL-CreditNote-2.1.xsd"),
                ["08"] = ("DebitNote", UblNamespaces.DebitNote, "UBL-DebitNote-2.1.xsd")
            };

        public UblXsdSchemaValidator(IHostEnvironment environment)
            : this(Path.Combine(
                environment?.ContentRootPath ?? throw new ArgumentNullException(nameof(environment)),
                "Resources", "Facturacion", "Ubl21", "xsd"))
        {
        }

        public UblXsdSchemaValidator(string schemaRoot)
        {
            if (string.IsNullOrWhiteSpace(schemaRoot))
                throw new ArgumentException("La raíz de schemas UBL es obligatoria.", nameof(schemaRoot));

            _schemaRoot = Path.GetFullPath(schemaRoot);
            _schemas = new Lazy<XmlSchemaSet>(
                BuildSchemaSet,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public async Task<XmlValidationResult> ValidarAsync(
            Stream xmlStream,
            string tipoComprobante,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (xmlStream is null || (xmlStream.CanSeek && xmlStream.Length == 0))
                return XmlValidationResult.Invalido(new[] { "El stream XML está vacío." });

            if (!SchemaByType.TryGetValue(tipoComprobante ?? string.Empty, out var expectedSchema))
                return XmlValidationResult.Invalido(new[] { "El tipo de comprobante no tiene un schema UBL autorizado." });

            XmlSchemaSet schemas;
            try
            {
                schemas = _schemas.Value;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException or XmlSchemaException)
            {
                return XmlValidationResult.Invalido(new[]
                {
                    "No fue posible cargar o compilar el paquete local de schemas UBL 2.1."
                });
            }

            var originalPosition = xmlStream.CanSeek ? xmlStream.Position : 0;
            var errors = new List<string>();

            try
            {
                var settings = new XmlReaderSettings
                {
                    Async = true,
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    ValidationType = ValidationType.Schema,
                    Schemas = schemas,
                    ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings,
                    MaxCharactersInDocument = MaxXmlCharacters,
                    MaxCharactersFromEntities = 0
                };
                settings.ValidationEventHandler += (_, args) =>
                    errors.Add(FormatearError(args.Exception, args.Severity));

                using var reader = XmlReader.Create(xmlStream, settings);
                var rootChecked = false;
                while (await reader.ReadAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!rootChecked && reader.NodeType == XmlNodeType.Element)
                    {
                        rootChecked = true;
                        if (!string.Equals(reader.LocalName, expectedSchema.Root, StringComparison.Ordinal) ||
                            !string.Equals(reader.NamespaceURI, expectedSchema.Namespace, StringComparison.Ordinal))
                        {
                            errors.Add("El elemento raíz no corresponde al tipo de comprobante declarado.");
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (XmlException ex)
            {
                errors.Add($"XML inválido en línea {ex.LineNumber}, posición {ex.LinePosition}.");
            }
            catch (XmlSchemaValidationException ex)
            {
                errors.Add(FormatearError(ex, XmlSeverityType.Error));
            }
            finally
            {
                if (xmlStream.CanSeek)
                    xmlStream.Position = originalPosition;
            }

            return errors.Count == 0
                ? XmlValidationResult.Valido()
                : XmlValidationResult.Invalido(errors.Distinct(StringComparer.Ordinal).ToArray());
        }

        private XmlSchemaSet BuildSchemaSet()
        {
            if (!Directory.Exists(_schemaRoot))
                throw new DirectoryNotFoundException("No se encontró el paquete local de schemas UBL 2.1.");

            var resolver = new AllowlistedSchemaResolver(_schemaRoot);
            var set = new XmlSchemaSet { XmlResolver = resolver };
            var readerSettings = new XmlReaderSettings
            {
                // El XSD oficial de XMLDSig incluido por OASIS usa un
                // subconjunto DTD interno. El resolver continúa restringido
                // al directorio local autorizado para impedir accesos remotos.
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = resolver,
                MaxCharactersInDocument = MaxXmlCharacters,
                MaxCharactersFromEntities = 0
            };

            foreach (var schema in SchemaByType.Values.Distinct())
            {
                var path = Path.Combine(_schemaRoot, "maindoc", schema.File);
                if (!File.Exists(path))
                    throw new FileNotFoundException("Falta un schema UBL 2.1 requerido.");

                using var reader = XmlReader.Create(path, readerSettings);
                set.Add(schema.Namespace, reader);
            }

            set.Compile();
            return set;
        }

        private static string FormatearError(XmlSchemaException? exception, XmlSeverityType severity)
        {
            var prefix = severity == XmlSeverityType.Warning ? "Advertencia XSD" : "Error XSD";
            if (exception is null)
                return prefix + ".";

            return exception.LineNumber > 0
                ? $"{prefix} en línea {exception.LineNumber}, posición {exception.LinePosition}: {exception.Message}"
                : $"{prefix}: {exception.Message}";
        }

        private sealed class AllowlistedSchemaResolver : XmlResolver
        {
            private readonly string _rootWithSeparator;

            public AllowlistedSchemaResolver(string root)
            {
                _rootWithSeparator = Path.GetFullPath(root)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;
            }

            public override ICredentials? Credentials
            {
                set { }
            }

            public override Uri ResolveUri(Uri? baseUri, string? relativeUri)
            {
                if (string.IsNullOrWhiteSpace(relativeUri))
                    throw new XmlException("La referencia de schema está vacía.");

                if (Uri.TryCreate(relativeUri, UriKind.Absolute, out var absolute) && !absolute.IsFile)
                    throw new XmlException("La resolución remota de schemas está prohibida.");

                var resolved = base.ResolveUri(baseUri, relativeUri);
                if (!resolved.IsFile)
                    throw new XmlException("Solo se permiten schemas locales autorizados.");

                EnsureAllowed(resolved.LocalPath);
                return resolved;
            }

            public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
            {
                if (!absoluteUri.IsFile ||
                    (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream)))
                    throw new XmlException("La referencia de schema no está autorizada.");

                var path = EnsureAllowed(absoluteUri.LocalPath);
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            private string EnsureAllowed(string path)
            {
                var fullPath = Path.GetFullPath(path);
                if (!fullPath.StartsWith(_rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                    throw new XmlException("La referencia de schema está fuera del paquete autorizado.");

                return fullPath;
            }
        }
    }
}
