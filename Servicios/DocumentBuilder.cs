using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Modelos.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios
{
    public class DocumentBuilder : IDocumentBuilder
    {
        public Task<DocumentoElectronicoDTO> ConstruirDesdeVentaAsync(
            Venta venta,
            Comprobante comprobante,
            CancellationToken cancellationToken = default)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));
            if (comprobante == null) throw new ArgumentNullException(nameof(comprobante));
            cancellationToken.ThrowIfCancellationRequested();

            if (venta.IdEmpresa != comprobante.IdEmpresa)
                throw new InvalidOperationException("La empresa de la venta no coincide con la del comprobante.");
            if (venta.Cliente == null)
                throw new InvalidOperationException("La venta debe tener un cliente asociado para generar el comprobante.");
            if (venta.VentaDetalles == null || !venta.VentaDetalles.Any())
                throw new InvalidOperationException("La venta debe tener al menos un detalle.");

            var tipoComprobante = TipoComprobanteFiscal.Normalizar(comprobante.TipoComprobante);
            if (tipoComprobante == TipoComprobanteFiscal.Factura && venta.Cliente.TipoDocumento != "RUC")
                throw new InvalidOperationException("Para emitir Factura el cliente debe contar con RUC.");

            return Task.FromResult(ConstruirDocumentoBase(venta, comprobante, tipoComprobante));
        }

        public Task<DocumentoElectronicoDTO> ConstruirDesdeVentaAsync(
            Venta venta,
            Comprobante comprobante,
            Empresa empresa,
            CancellationToken cancellationToken = default)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));
            if (comprobante == null) throw new ArgumentNullException(nameof(comprobante));
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));

            cancellationToken.ThrowIfCancellationRequested();
            
            // Validaciones estructurales básicas
            if (venta.IdEmpresa != comprobante.IdEmpresa)
                throw new InvalidOperationException("La empresa de la venta no coincide con la del comprobante.");

            if (empresa.IdEmpresa <= 0 ||
                empresa.IdEmpresa != venta.IdEmpresa ||
                empresa.IdEmpresa != comprobante.IdEmpresa)
                throw new InvalidOperationException("La empresa emisora no coincide con la venta y el comprobante.");

            var rucEmisor = empresa.RucDocumento?.Trim();
            if (string.IsNullOrWhiteSpace(rucEmisor) || rucEmisor.Length != 11 || !rucEmisor.All(char.IsDigit))
                throw new InvalidOperationException("La empresa emisora debe contar con un RUC válido de 11 dígitos.");

            var razonSocialEmisor = empresa.RazonSocial?.Trim();
            if (string.IsNullOrWhiteSpace(razonSocialEmisor))
                throw new InvalidOperationException("La razón social de la empresa emisora es obligatoria.");

            var tipoComprobante = TipoComprobanteFiscal.Normalizar(comprobante.TipoComprobante);
            
            if (venta.Cliente == null)
                throw new InvalidOperationException("La venta debe tener un cliente asociado para generar el comprobante.");
                
            if (venta.VentaDetalles == null || !venta.VentaDetalles.Any())
                throw new InvalidOperationException("La venta debe tener al menos un detalle.");

            // Si es Factura, el cliente debe tener RUC (Ejemplo genérico). 
            // En implementaciones finales esto puede ser configurable por empresa.
            if (tipoComprobante == TipoComprobanteFiscal.Factura && venta.Cliente.TipoDocumento != "RUC")
            {
                throw new InvalidOperationException("Para emitir Factura el cliente debe contar con RUC.");
            }

            var doc = ConstruirDocumentoBase(venta, comprobante, tipoComprobante);
            doc.Emisor = new EmisorDocumentoDTO
                {
                    IdEmpresa = empresa.IdEmpresa,
                    Ruc = rucEmisor,
                    RazonSocial = razonSocialEmisor,
                    Direccion = string.IsNullOrWhiteSpace(empresa.DireccionFiscal)
                        ? null
                        : empresa.DireccionFiscal.Trim()
                };

            return Task.FromResult(doc);
        }

        private DocumentoElectronicoDTO ConstruirDocumentoBase(
            Venta venta,
            Comprobante comprobante,
            string tipoComprobante)
        {
            return new DocumentoElectronicoDTO
            {
                IdComprobante = comprobante.IdComprobante,
                IdEmpresa = comprobante.IdEmpresa,
                TipoComprobante = tipoComprobante,
                Serie = comprobante.SerieFiscal,
                Correlativo = comprobante.CorrelativoFiscal ?? 0,
                FechaEmision = venta.FechaVenta,
                Moneda = venta.Moneda,
                VersionNormativa = comprobante.VersionNormativa
                    ?? throw new InvalidOperationException("El comprobante no tiene versión normativa SUNAT registrada."),
                TipoOperacionSunat = comprobante.TipoOperacionSunat ?? string.Empty,
                TotalGravado = venta.Subtotal,
                TotalIgv = venta.Impuesto,
                TotalDescuento = venta.DescuentoTotal,
                ImporteTotal = venta.Total,
                Cliente = ConstruirCliente(venta.Cliente),
                Detalles = venta.VentaDetalles.Select(ConstruirDetalle).ToList(),
                DocumentoRelacionado = tipoComprobante is TipoComprobanteFiscal.NotaCredito or TipoComprobanteFiscal.NotaDebito
                    ? ConstruirDocumentoRelacionado(comprobante)
                    : null
            };
        }

        private ClienteDocumentoDTO ConstruirCliente(Cliente cliente)
        {
            // Determinar razón social (si es jurídica usa RazonSocial, si es natural usa Nombres + Apellidos)
            string razonSocial = cliente.TipoPersona == "J" 
                ? (cliente.RazonSocial ?? string.Empty) 
                : $"{cliente.Nombres} {cliente.Apellidos}".Trim();

            // Usamos la primera dirección disponible, en la vida real esto puede ser configurado
            string? direccion = cliente.Direcciones?.FirstOrDefault(d => d.EsPrincipal)?.DireccionCompleta 
                                ?? cliente.Direcciones?.FirstOrDefault()?.DireccionCompleta;

            return new ClienteDocumentoDTO
            {
                TipoDocumentoIdentidad = cliente.TipoDocumento,
                NumeroDocumento = cliente.NumeroDocumento,
                RazonSocial = razonSocial,
                Direccion = direccion,
                Email = cliente.Email
            };
        }

        private DetalleDocumentoDTO ConstruirDetalle(VentaDetalle detalle, int index)
        {
            // NOTE: ClasificacionTributariaAplicada, CodigoUnidadMedidaAplicada are no longer persisted
            // Using Producto defaults instead
            var productoUnidad = detalle.Producto?.UnidadMedida?.Codigo?.Trim().ToUpperInvariant() ?? "ZZ";
            var productoClasificacion = detalle.Producto?.ClasificacionTributaria?.Trim().ToUpperInvariant() ?? "GRA";
            
            var fiscal = TraducirClasificacionFiscal(productoClasificacion);
            
            return new DetalleDocumentoDTO
            {
                Secuencia = index + 1,
                CodigoProducto = detalle.Producto?.SKU ?? detalle.IdProducto.ToString(),
                Descripcion = detalle.Producto?.NombreComercial ?? "Producto desconocido",
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                ValorVenta = detalle.BaseImponible,
                PorcentajeImpuesto = detalle.PorcentajeImpuesto,
                MontoImpuesto = detalle.MontoImpuesto,
                ClasificacionTributaria = productoClasificacion,
                UnidadMedidaSunat = productoUnidad,
                CodigoAfectacionIgv = fiscal.Afectacion,
                CodigoTributo = fiscal.Tributo,
                NombreTributo = fiscal.Nombre,
                CodigoInternacionalTributo = fiscal.CodigoInternacional,
                TipoPrecio = "01"
            };
        }

        private static DocumentoRelacionadoDTO ConstruirDocumentoRelacionado(Comprobante comprobante)
        {
            if (string.IsNullOrWhiteSpace(comprobante.TipoDocumentoOrigen) ||
                string.IsNullOrWhiteSpace(comprobante.SerieDocumentoOrigen) ||
                comprobante.CorrelativoDocumentoOrigen is null or <= 0 ||
                string.IsNullOrWhiteSpace(comprobante.CodigoMotivoNota) ||
                string.IsNullOrWhiteSpace(comprobante.SustentoNota))
                throw new InvalidOperationException("La nota no conserva la referencia fiscal completa del documento afectado.");

            return new DocumentoRelacionadoDTO
            {
                TipoDocumento = comprobante.TipoDocumentoOrigen,
                Serie = comprobante.SerieDocumentoOrigen,
                Correlativo = comprobante.CorrelativoDocumentoOrigen.Value,
                CodigoMotivo = comprobante.CodigoMotivoNota,
                Sustento = comprobante.SustentoNota
            };
        }

        private static (string Afectacion, string Tributo, string Nombre, string CodigoInternacional)
            TraducirClasificacionFiscal(string? clasificacion)
        {
            return clasificacion?.Trim().ToUpperInvariant() switch
            {
                "GRAVADO" or "GRA" => ("10", "1000", "IGV", "VAT"),
                "EXONERADO" or "EXO" => ("20", "9997", "EXO", "VAT"),
                "INAFECTO" or "INA" => ("30", "9998", "INA", "FRE"),
                _ => throw new InvalidOperationException(
                    "La clasificación tributaria no tiene una traducción SUNAT inequívoca registrada.")
            };
        }
    }
}
