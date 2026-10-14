using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class MovimientoInventarioConfiguracion : IEntityTypeConfiguration<MovimientoInventario>
    {
        public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
        {
            builder.ToTable("MovimientosInventario", "dbo");
            builder.HasKey(e => e.IdMovimiento);
            builder.Property(e => e.Cantidad).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.SaldoAnterior).HasColumnType("decimal(18,2)");
            builder.Property(e => e.SaldoActual).HasColumnType("decimal(18,2)");
            builder.Property(e => e.FechaHora).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioResponsable).HasMaxLength(50).IsRequired();
            builder.Property(e => e.MotivoObservacion).HasMaxLength(250);
            builder.Property(e => e.IdReferenciaOrigen).HasMaxLength(100);
            
            builder.HasIndex(e => new { e.IdProducto, e.IdAlmacen, e.IdUbicacion, e.FechaHora });
            builder.HasIndex(e => e.FechaHora);

            builder.HasOne(e => e.Producto).WithMany(p => p.Movimientos).HasForeignKey(e => e.IdProducto).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.TipoMovimiento).WithMany(t => t.Movimientos).HasForeignKey(e => e.IdTipoMovimiento).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Ubicacion).WithMany(u => u.Movimientos).HasForeignKey(e => new { e.IdUbicacion, e.IdAlmacen }).HasPrincipalKey(u => new { u.IdUbicacion, u.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}