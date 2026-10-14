using System;

namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class ComprobanteAuditoria
    {
        public long IdAuditoria { get; set; }
        public int IdEmpresa { get; set; }
        public Guid IdComprobante { get; set; }
        public string? EstadoProcesamientoAnterior { get; set; }
        public string? EstadoProcesamientoNuevo { get; set; }
        public string? EstadoFiscalAnterior { get; set; }
        public string? EstadoFiscalNuevo { get; set; }
        public string Evento { get; set; } = null!;
        public Guid? ExecutionToken { get; set; }
        public DateTime FechaEvento { get; set; }
        public string? Detalle { get; set; }
        
        public Comprobante Comprobante { get; set; } = null!;
    }
}
