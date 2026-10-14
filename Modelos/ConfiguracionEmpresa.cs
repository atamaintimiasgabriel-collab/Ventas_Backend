namespace SistemaVentasMotorrepuestos.API.Modelos;

/// <summary>Parámetros operativos globales. No contiene datos fiscales ni secretos.</summary>
public sealed class ConfiguracionEmpresa
{
    public int IdEmpresa { get; set; }
    public string Moneda { get; set; } = "PEN";
    public string Idioma { get; set; } = "es-PE";
    public string ZonaHoraria { get; set; } = "America/Lima";
    public string FormatoFecha { get; set; } = "dd/MM/yyyy";
    public string FormatoHora { get; set; } = "HH:mm";
    public bool ControlStockNegativo { get; set; }
    public bool AlertasStock { get; set; } = true;
    public bool RequiereCajaAbierta { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
