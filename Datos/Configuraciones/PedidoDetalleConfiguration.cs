using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class PedidoDetalleConfiguration : IEntityTypeConfiguration<PedidoDetalle>
    {
        public void Configure(EntityTypeBuilder<PedidoDetalle> builder)
        {
            builder.ToTable("PedidoDetalles", "dbo");
            builder.HasKey(d => d.IdDetallePedido);

            builder.Property(d => d.Cantidad).HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(d => d.Descuento).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(d => d.BaseImponible).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(d => d.ClasificacionTributariaAplicada).HasMaxLength(10);
            builder.Property(d => d.ConceptoTributarioAplicado).HasMaxLength(20);
            builder.Property(d => d.Subtotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(d => d.PorcentajeImpuesto).HasColumnType("decimal(5,2)").IsRequired();
            builder.Property(d => d.MontoImpuesto).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(d => d.TotalLinea).HasColumnType("decimal(18,2)").HasDefaultValue(0);

            builder.HasOne(d => d.Producto)
                   .WithMany()
                   .HasForeignKey(d => d.IdProducto)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
