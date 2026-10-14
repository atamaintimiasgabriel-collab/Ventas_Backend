using System.ComponentModel.DataAnnotations;

namespace SistemaVentasMotorrepuestos.API.DTOs.Configuracion;

public sealed class ActualizarSistemaRequest
{
    [Required, RegularExpression("^(PEN|USD)$")] public string Moneda { get; set; } = "PEN";
    [Required, RegularExpression("^(es-PE|en-US)$")] public string Idioma { get; set; } = "es-PE";
    [Required, MaxLength(64)] public string ZonaHoraria { get; set; } = "America/Lima";
    [Required, MaxLength(32)] public string FormatoFecha { get; set; } = "dd/MM/yyyy";
    [Required, MaxLength(32)] public string FormatoHora { get; set; } = "HH:mm";
    public bool ControlStockNegativo { get; set; }
    public bool AlertasStock { get; set; }
    public bool RequiereCajaAbierta { get; set; }
}

public sealed class ActualizarEmpresaRequest
{
    [Required, MaxLength(150)] public string RazonSocial { get; set; } = string.Empty;
    [Required, RegularExpression("^\\d{11}$")] public string RucDocumento { get; set; } = string.Empty;
    [MaxLength(250)] public string? DireccionFiscal { get; set; }
    [MaxLength(20)] public string? Telefono { get; set; }
    [EmailAddress, MaxLength(100)] public string? EmailContacto { get; set; }
    [MaxLength(150)] public string? NombreComercial { get; set; }
    [MaxLength(200)] public string? SitioWeb { get; set; }
}

public sealed class ActualizarPreferenciaUsuarioRequest
{
    public bool Autoguardado { get; set; }
    public bool Notificaciones { get; set; }
    public bool ReproducirSonidos { get; set; }
    [Required, RegularExpression("^(blue|emerald|red|orange|violet|pink)$")] public string Tema { get; set; } = "blue";
    public bool ModoOscuro { get; set; }
    public bool Animaciones { get; set; }
    public bool MostrarImagenesCatalogo { get; set; }
    [Required, RegularExpression("^(normal|pequena|grande)$")] public string TamanoFuente { get; set; } = "normal";
    [Required, RegularExpression("^(comoda|compacta)$")] public string DensidadInformacion { get; set; } = "comoda";
}

public sealed class GuardarSucursalRequest
{
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [MaxLength(10)] public string? CodigoEstablecimiento { get; set; }
    [MaxLength(250)] public string? Direccion { get; set; }
    [MaxLength(20)] public string? Telefono { get; set; }
}

public sealed class GuardarUsuarioConfiguracionRequest
{
    [Required, MaxLength(50)] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = string.Empty;
    [MinLength(8), MaxLength(128)] public string? Password { get; set; }
    [MinLength(1)] public List<int> RolesIds { get; set; } = [];
    [MinLength(1)] public List<int> SucursalesIds { get; set; } = [];
    public int? IdSucursalPrincipal { get; set; }
}

public sealed class GuardarRolConfiguracionRequest
{
    [Required, MaxLength(50)] public string NombreRol { get; set; } = string.Empty;
    [MaxLength(255)] public string? Descripcion { get; set; }
    public List<int> PermisosIds { get; set; } = [];
}

public sealed class GuardarSerieFiscalRequest
{
    [Required, MaxLength(4)] public string TipoComprobante { get; set; } = string.Empty;
    [Required, RegularExpression("^[A-Za-z0-9]{1,4}$")] public string Serie { get; set; } = string.Empty;
    [Range(1, long.MaxValue)] public long CorrelativoActual { get; set; } = 1;
    public int? IdSucursal { get; set; }
}

public sealed class ActualizarImpuestosRequest
{
    [Range(0, 100)] public decimal TasaIgv { get; set; } = 18m;
    public bool PreciosIncluyenImpuestos { get; set; } = true;
}
