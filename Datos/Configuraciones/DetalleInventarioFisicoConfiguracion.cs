using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class DetalleInventarioFisicoConfiguracion : IEntityTypeConfiguration<DetalleInventarioFisico>
    {
        public void Configure(EntityTypeBuilder<DetalleInventarioFisico> builder)
        {
            builder.ToTable("DetalleInventarioFisico", "dbo");
            builder.HasKey(e => e.IdDetalleInventario);
            builder.Property(e => e.StockSistema).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.CantidadFisica).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.Diferencia).HasColumnType("decimal(18,2)").ValueGeneratedOnAddOrUpdate();
            builder.Property(e => e.Observacion).HasMaxLength(200);
            
            builder.HasIndex(e => new { e.IdInventarioFisico, e.IdProducto, e.IdUbicacion }).IsUnique();

            builder.HasOne(e => e.InventarioFisico).WithMany(i => i.Detalles).HasForeignKey(e => new { e.IdInventarioFisico, e.IdAlmacen }).HasPrincipalKey(i => new { i.IdInventarioFisico, i.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Producto).WithMany(p => p.DetallesInventarioFisico).HasForeignKey(e => e.IdProducto).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Ubicacion).WithMany(u => u.DetallesInventarioFisico).HasForeignKey(e => new { e.IdUbicacion, e.IdAlmacen }).HasPrincipalKey(u => new { u.IdUbicacion, u.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}