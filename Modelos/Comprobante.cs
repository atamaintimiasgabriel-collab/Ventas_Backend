using System;
using System.Collections.Generic;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class Comprobante
    {
        public Guid IdComprobante { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdSucursal { get; set; }
        public int? IdVenta { get; set; }
        public Guid? IdComprobanteOrigen { get; set; }
        
        public string TipoComprobante { get; set; } = null!;
        public string SerieFiscal { get; set; } = null!;
        public long? CorrelativoFiscal { get; set; }
        public string? VersionNormativa { get; set; }
        public string? TipoOperacionSunat { get; set; }
        public string? TipoDocumentoOrigen { get; set; }
        public string? SerieDocumentoOrigen { get; set; }
        public long? CorrelativoDocumentoOrigen { get; set; }
        public string? CodigoMotivoNota { get; set; }
        public string? SustentoNota { get; set; }
        
        public string EstadoProcesamiento { get; set; } = null!;
        public string? WorkerId { get; set; }
        public Guid? ExecutionToken { get; set; }
        public DateTime? LeaseHasta { get; set; }
        public int NumeroIntento { get; set; }
        public DateTime? FechaProximoIntento { get; set; }
        
        public string EstadoFiscal { get; set; } = null!;
        
        public string? IdOperacionExterna { get; set; }
        public string? IdDocumentoExterno { get; set; }
        
        public string? StorageKeyXml { get; set; }
        public string? StorageKeyCdr { get; set; }
        public string? StorageKeyPdf { get; set; }
        
        public DateTime FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
        
        public byte[] RowVersion { get; set; } = null!;
        
        public Empresa Empresa { get; set; } = null!;
        public Sucursal? Sucursal { get; set; }
        public Venta? Venta { get; set; }
        public Comprobante? ComprobanteOrigen { get; set; }
        
        public ICollection<ComprobanteAuditoria> Auditorias { get; set; } = new List<ComprobanteAuditoria>();
    }
}
