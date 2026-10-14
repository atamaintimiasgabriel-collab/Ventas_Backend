namespace SistemaVentasMotorrepuestos.API.Modelos;

/// <summary>Ámbito explícito de sucursales permitido a cada usuario.</summary>
public sealed class UsuarioSucursal
{
    public int IdUsuario { get; set; }
    public int IdSucursal { get; set; }
    public bool EsPrincipal { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
}
