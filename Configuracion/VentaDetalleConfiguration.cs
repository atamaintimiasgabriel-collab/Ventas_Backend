using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
    {
        public void Configure(EntityTypeBuilder<VentaDetalle> builder)
        {
            builder.ToTable("VentaDetalles");

            builder.HasKey(d => d.IdDetalleVenta);

            builder.Property(d => d.Cantidad).HasColumnType("decimal(18,2)");
            builder.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
            builder.Property(d => d.CostoUnitario).HasColumnType("decimal(18,2)");
            builder.Property(d => d.Descuento).HasColumnType("decimal(18,2)");
            builder.Property(d => d.BaseImponible).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(d => d.PorcentajeImpuesto).HasColumnType("decimal(18,2)");
            builder.Property(d => d.MontoImpuesto).HasColumnType("decimal(18,2)");
            builder.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
            builder.Property(d => d.TotalLinea).HasColumnType("decimal(18,2)");

            // Relaciones
            builder.HasOne(d => d.Venta)
                .WithMany(v => v.VentaDetalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
