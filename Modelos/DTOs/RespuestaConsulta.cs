namespace SistemaVentasMotorrepuestos.API.Modelos.DTOs
{
    public class RespuestaConsulta
    {
        public bool EsExitoso { get; set; }
        public string EstadoFiscal { get; set; } = null!;
        public string? MensajeError { get; set; }
        public string? CodigoError { get; set; }
    }
}
