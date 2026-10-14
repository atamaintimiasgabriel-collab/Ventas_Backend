namespace SistemaVentasMotorrepuestos.API.Modelos;

/// <summary>Preferencias no sensibles y de ámbito individual.</summary>
public sealed class PreferenciaUsuario
{
    public int IdUsuario { get; set; }
    public bool Autoguardado { get; set; } = true;
    public bool Notificaciones { get; set; } = true;
    public bool ReproducirSonidos { get; set; } = true;
    public string Tema { get; set; } = "blue";
    public bool ModoOscuro { get; set; }
    public bool Animaciones { get; set; } = true;
    public bool MostrarImagenesCatalogo { get; set; } = true;
    public string TamanoFuente { get; set; } = "normal";
    public string DensidadInformacion { get; set; } = "comoda";
    public DateTime FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
