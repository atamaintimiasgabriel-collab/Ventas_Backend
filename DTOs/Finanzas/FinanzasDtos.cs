using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Finanzas
{
    public class CajaResponse
    {
        public int IdCaja { get; set; }
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class AperturaCajaResponse
    {
        public int IdApertura { get; set; }
        public int IdCaja { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
        public decimal SaldoEsperado { get; set; }
        public decimal? SaldoContado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
    }

    public class AbrirCajaRequest
    {
        [Range(1, int.MaxValue)]
        public int IdCaja { get; set; }

        [Range(0, 999999999)]
        public decimal SaldoInicial { get; set; }
    }

    public class CerrarCajaRequest
    {
        [Range(0, 999999999)]
        public decimal SaldoContado { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }

    public class MedioPagoResponse
    {
        public int IdMedio { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CanalPagoResponse
    {
        public int IdCanal { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ProveedorPagoResponse
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class RegistrarPagoVentaRequest
    {
        [Range(1, int.MaxValue)]
        public int IdVenta { get; set; }

        [Range(1, int.MaxValue)]
        public int IdMedioPago { get; set; }

        [Range(1, int.MaxValue)]
        public int IdCanalPago { get; set; }

        public int? IdProveedor { get; set; }

        [Range(0.01, 999999999)]
        public decimal Monto { get; set; }

        [StringLength(3, MinimumLength = 3)]
        public string Moneda { get; set; } = "PEN";

        [StringLength(100)]
        public string? ReferenciaExterna { get; set; }

        [StringLength(100)]
        public string? IdempotencyKey { get; set; }

        // Datos opcionales que ya existen en la tabla Pagos (tarjeta: adquirente/autorizacion/ultimos 4; transferencia: banco en Adquirente).
        [StringLength(100)]
        public string? Adquirente { get; set; }

        [StringLength(100)]
        public string? CodigoAutorizacion { get; set; }

        [StringLength(4)]
        public string? Ultimos4Digitos { get; set; }

        public int? IdAperturaCaja { get; set; }
    }

    public class PagoVentaResponse
    {
        public int IdPago { get; set; }
        public int IdAplicacion { get; set; }
        public int IdObligacion { get; set; }
        public int? IdMovimientoCaja { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal MontoAplicado { get; set; }
        public decimal TotalAplicado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoObligacion { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
    }

    public class EstadoFinancieroVentaResponse
    {
        public int IdVenta { get; set; }
        public int IdObligacion { get; set; }
        public decimal Total { get; set; }
        public decimal DeudaOriginal { get; set; }
        public decimal TotalAplicado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoObligacion { get; set; } = string.Empty;
        public List<PagoFinancieroResponse> Pagos { get; set; } = new();
    }

    public class PagoFinancieroResponse
    {
        public int IdPago { get; set; }
        public decimal Monto { get; set; }
        public string MedioPago { get; set; } = string.Empty;
        public string CanalPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class MovimientoCajaResponse
    {
        public int IdMovCaja { get; set; }
        public int IdApertura { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? IdOrigen { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class RegistrarMovimientoCajaRequest
    {
        [Range(1, int.MaxValue)] public int IdApertura { get; set; }
        [Required, StringLength(20)] public string Tipo { get; set; } = string.Empty;
        [Range(0.01, 999999999)] public decimal Monto { get; set; }
        [Required, StringLength(500)] public string Concepto { get; set; } = string.Empty;
        [StringLength(100)] public string? Referencia { get; set; }
    }

    public class CajaResumenResponse
    {
        public decimal SaldoActual { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public int TotalMovimientos { get; set; }
        public AperturaCajaResponse? Apertura { get; set; }
        public List<MovimientoCajaResponse> Movimientos { get; set; } = new();
        public decimal VentasDelDia { get; set; }
        public int ComprobantesDelDia { get; set; }
        public List<CajaSerieDiariaResponse> UltimosSieteDias { get; set; } = new();
        public List<CajaDistribucionResponse> Distribucion { get; set; } = new();
        public List<CajaDistribucionResponse> DistribucionMediosPago { get; set; } = new();
    }

    public class CajaSerieDiariaResponse
    {
        public DateTime Fecha { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
    }

    public class CajaDistribucionResponse
    {
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
