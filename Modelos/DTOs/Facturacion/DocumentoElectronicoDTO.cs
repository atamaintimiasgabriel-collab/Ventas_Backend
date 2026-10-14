using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion
{
    // Modelo abstracto intermedio, desacoplado de Venta y de Proveedores específicos (SUNAT, OSE, PSE)
    public class DocumentoElectronicoDTO
    {
        public Guid IdComprobante { get; set; }
        public int IdEmpresa { get; set; }
        public string TipoComprobante { get; set; } = null!; // Factura, Boleta, NC, ND
        public string Serie { get; set; } = null!;
        public long Correlativo { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Moneda { get; set; } = null!;
        public string VersionNormativa { get; set; } = null!;
        public string TipoOperacionSunat { get; set; } = null!;

        public EmisorDocumentoDTO Emisor { get; set; } = null!;
        
        public ClienteDocumentoDTO Cliente { get; set; } = null!;
        
        // Snapshot tributario totalizado
        public decimal TotalGravado { get; set; }
        public decimal TotalIgv { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal ImporteTotal { get; set; }
        
        public List<DetalleDocumentoDTO> Detalles { get; set; } = new List<DetalleDocumentoDTO>();
        public DocumentoRelacionadoDTO? DocumentoRelacionado { get; set; }

        // Datos solo de presentacion (vista previa/PDF/ticket); no participan en el XML fiscal.
        public ImpresionExtraDTO? Impresion { get; set; }
    }

    public class ImpresionExtraDTO
    {
        public string NumeroVenta { get; set; } = string.Empty;
        public string? Sucursal { get; set; }
        public string? TelefonoEmpresa { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? Cajero { get; set; }
        public string? EstadoFiscal { get; set; }
        public bool CorrelativoPendiente { get; set; }
        public List<PagoImpresionDTO> Pagos { get; set; } = new List<PagoImpresionDTO>();
        public decimal? MontoRecibido { get; set; }
        public decimal? Vuelto { get; set; }
    }

    public class PagoImpresionDTO
    {
        public string MedioPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
        public string? Adquirente { get; set; }
        public string? CodigoAutorizacion { get; set; }
        public string? Ultimos4Digitos { get; set; }
        public DateTime? Fecha { get; set; }
    }

    public class ClienteDocumentoDTO
    {
        public string TipoDocumentoIdentidad { get; set; } = null!;
        public string NumeroDocumento { get; set; } = null!;
        public string RazonSocial { get; set; } = null!;
        public string? Direccion { get; set; }
        public string? Email { get; set; }
    }

    public class DetalleDocumentoDTO
    {
        public int Secuencia { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } // Incluye impuestos dependiendo de configuración
        public decimal Descuento { get; set; }
        public decimal ValorVenta { get; set; } // Base imponible
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
        public string ClasificacionTributaria { get; set; } = null!; // Gravado, Exonerado, Inafecto
        public string UnidadMedidaSunat { get; set; } = null!;
        public string CodigoAfectacionIgv { get; set; } = null!;
        public string CodigoTributo { get; set; } = null!;
        public string NombreTributo { get; set; } = null!;
        public string CodigoInternacionalTributo { get; set; } = null!;
        public string TipoPrecio { get; set; } = null!;
    }

    public class DocumentoRelacionadoDTO
    {
        public string TipoDocumento { get; set; } = null!;
        public string Serie { get; set; } = null!;
        public long Correlativo { get; set; }
        public string CodigoMotivo { get; set; } = null!;
        public string Sustento { get; set; } = null!;
    }
}
