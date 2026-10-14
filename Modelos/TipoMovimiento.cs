namespace SistemaVentasMotorrepuestos.API.Modelos
{
    public class TipoMovimiento
    {
        public int IdTipoMovimiento { get; set; }
        public string Codigo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int Naturaleza { get; set; }
        public bool Activo { get; set; }

        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}