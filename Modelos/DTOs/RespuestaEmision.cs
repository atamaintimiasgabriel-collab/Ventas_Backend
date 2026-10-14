namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs
{
    public class RespuestaEmision
    {
        public bool EsExitoso { get; set; }
        public string EstadoFiscal { get; set; } = null!;
        public string? IdOperacionExterna { get; set; }
        public string? IdDocumentoExterno { get; set; }
        public string? MensajeError { get; set; }
        public string? CodigoError { get; set; }
    }
}
