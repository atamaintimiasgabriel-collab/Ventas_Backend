using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Ventas;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Modelos.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    public class DocumentoImpresionService : IDocumentoImpresionService
    {
        public const string Ticket = "TICKET";

        private readonly ApplicationDbContext _db;
        private readonly IPdfGenerator _pdfGenerator;

        public DocumentoImpresionService(ApplicationDbContext db, IPdfGenerator pdfGenerator)
        {
            _db = db;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<DocumentoElectronicoDTO> ObtenerDocumentoAsync(
            int idEmpresa, int idVenta, string tipo, decimal? montoRecibido, CancellationToken cancellationToken = default)
        {
            var tipoNormalizado = NormalizarTipo(tipo);

            var venta = await _db.Ventas.AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Empresa)
                .Include(v => v.Sucursal)
                .Include(v => v.VentaDetalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.IdEmpresa == idEmpresa && v.IdVenta == idVenta, cancellationToken);

            if (venta == null)
                throw new VentaNotFoundException(idVenta);

            Comprobante? comprobante = null;
            if (tipoNormalizado != Ticket)
            {
                comprobante = await _db.Comprobantes.AsNoTracking()
                    .Where(c => c.IdEmpresa == idEmpresa && c.IdVenta == idVenta && c.TipoComprobante == tipoNormalizado
                                && c.EstadoFiscal != "RECHAZADO" && c.EstadoFiscal != "ANULADO")
                    .OrderByDescending(c => c.FechaCreacion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (comprobante == null)
                    throw new VentaDomainException(
                        $"La venta no tiene una {(tipoNormalizado == TipoComprobanteFiscal.Factura ? "factura" : "boleta")} emitida.");
            }

            var cajero = await _db.Usuarios.AsNoTracking()
                .Where(u => u.IdUsuario == venta.IdUsuarioVendedor)
                .Select(u => u.Username)
                .FirstOrDefaultAsync(cancellationToken);

            var pagos = await (from o in _db.ObligacionesCxC.AsNoTracking()
                               join a in _db.AplicacionesPago.AsNoTracking() on o.IdObligacion equals a.IdObligacion
                               where o.IdEmpresa == idEmpresa && o.IdVentaOrigen == idVenta
                               orderby a.Fecha
                               select new PagoImpresionDTO
                               {
                                   MedioPago = a.Pago.MedioPago.Nombre,
                                   Monto = a.MontoAplicado,
                                   Referencia = a.Pago.ReferenciaExterna,
                                   Adquirente = a.Pago.Adquirente,
                                   CodigoAutorizacion = a.Pago.CodigoAutorizacion,
                                   Ultimos4Digitos = a.Pago.Ultimos4Digitos,
                                   Fecha = a.Pago.FechaConfirmacion ?? a.Fecha
                               }).ToListAsync(cancellationToken);

            var cliente = venta.Cliente;
            var razonSocial = cliente.TipoPersona == "J"
                ? (cliente.RazonSocial ?? string.Empty)
                : $"{cliente.Nombres} {cliente.Apellidos}".Trim();
            if (string.IsNullOrWhiteSpace(razonSocial))
                razonSocial = cliente.NombreComercial ?? "Cliente";

            decimal? recibido = null;
            decimal? vuelto = null;
            var efectivo = pagos.Where(p => string.Equals(p.MedioPago, "EFECTIVO", StringComparison.OrdinalIgnoreCase)).Sum(p => p.Monto);
            if (montoRecibido.HasValue && efectivo > 0)
            {
                recibido = montoRecibido.Value;
                vuelto = Math.Max(0m, montoRecibido.Value - efectivo);
            }

            // FechaVenta se guarda en UTC; Peru no usa horario de verano (UTC-5). Kind sin especificar para que el JSON no lleve 'Z'.
            var fechaLocal = DateTime.SpecifyKind(venta.FechaVenta.AddHours(-5), DateTimeKind.Unspecified);

            return new DocumentoElectronicoDTO
            {
                IdComprobante = comprobante?.IdComprobante ?? Guid.Empty,
                IdEmpresa = idEmpresa,
                TipoComprobante = tipoNormalizado,
                Serie = comprobante?.SerieFiscal ?? string.Empty,
                Correlativo = comprobante?.CorrelativoFiscal ?? 0,
                FechaEmision = fechaLocal,
                Moneda = venta.Moneda,
                VersionNormativa = comprobante?.VersionNormativa ?? string.Empty,
                TipoOperacionSunat = comprobante?.TipoOperacionSunat ?? string.Empty,
                Emisor = new EmisorDocumentoDTO
                {
                    IdEmpresa = venta.Empresa.IdEmpresa,
                    Ruc = venta.Empresa.RucDocumento?.Trim() ?? string.Empty,
                    RazonSocial = venta.Empresa.RazonSocial?.Trim() ?? string.Empty,
                    Direccion = string.IsNullOrWhiteSpace(venta.Empresa.DireccionFiscal) ? null : venta.Empresa.DireccionFiscal.Trim()
                },
                Cliente = new ClienteDocumentoDTO
                {
                    TipoDocumentoIdentidad = cliente.TipoDocumento,
                    NumeroDocumento = cliente.NumeroDocumento,
                    RazonSocial = razonSocial,
                    Email = cliente.Email
                },
                TotalGravado = venta.Subtotal,
                TotalIgv = venta.Impuesto,
                TotalDescuento = venta.DescuentoTotal,
                ImporteTotal = venta.Total,
                Detalles = venta.VentaDetalles.Select((d, i) => new DetalleDocumentoDTO
                {
                    Secuencia = i + 1,
                    CodigoProducto = d.Producto?.SKU ?? d.IdProducto.ToString(),
                    Descripcion = d.Producto?.NombreComercial ?? "Producto",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    ValorVenta = d.Subtotal,
                    PorcentajeImpuesto = d.PorcentajeImpuesto,
                    MontoImpuesto = d.MontoImpuesto
                }).ToList(),
                Impresion = new ImpresionExtraDTO
                {
                    NumeroVenta = venta.NumeroVenta,
                    Sucursal = venta.Sucursal?.Nombre,
                    TelefonoEmpresa = venta.Empresa.Telefono,
                    TelefonoCliente = cliente.Telefono,
                    Cajero = cajero,
                    EstadoFiscal = comprobante?.EstadoFiscal,
                    CorrelativoPendiente = comprobante != null && comprobante.CorrelativoFiscal == null,
                    Pagos = pagos,
                    MontoRecibido = recibido,
                    Vuelto = vuelto
                }
            };
        }

        public async Task<(byte[] Contenido, string NombreArchivo)> GenerarPdfAsync(
            int idEmpresa, int idVenta, string tipo, decimal? montoRecibido, CancellationToken cancellationToken = default)
        {
            var documento = await ObtenerDocumentoAsync(idEmpresa, idVenta, tipo, montoRecibido, cancellationToken);
            var esTicket = documento.TipoComprobante == Ticket;

            var bytes = await _pdfGenerator.GenerarPdfAsync(
                documento,
                new ConfiguracionPdfDTO { Formato = esTicket ? FormatoPdf.Ticket : FormatoPdf.A4 });

            var nombre = esTicket
                ? $"TICKET-{documento.Impresion!.NumeroVenta}.pdf"
                : documento.Correlativo > 0
                    ? $"{documento.Serie}-{documento.Correlativo:D8}.pdf"
                    : $"{documento.Serie}-{documento.Impresion!.NumeroVenta}.pdf";

            return (bytes, nombre);
        }

        private static string NormalizarTipo(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new VentaDomainException("El tipo de documento es obligatorio (01, 03 o TICKET).");

            if (string.Equals(tipo.Trim(), Ticket, StringComparison.OrdinalIgnoreCase))
                return Ticket;

            try
            {
                var fiscal = TipoComprobanteFiscal.Normalizar(tipo);
                if (fiscal is TipoComprobanteFiscal.Factura or TipoComprobanteFiscal.Boleta)
                    return fiscal;
            }
            catch (ArgumentException)
            {
            }

            throw new VentaDomainException("El tipo de documento debe ser 01 (factura), 03 (boleta) o TICKET.");
        }
    }
}
