using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas");

            builder.HasKey(v => v.IdVenta);

            builder.Property(v => v.NumeroVenta).IsRequired().HasMaxLength(20);
            builder.Property(v => v.Origen).IsRequired().HasMaxLength(20);
            builder.Property(v => v.Estado).IsRequired().HasMaxLength(20);
            builder.Property(v => v.Moneda).IsRequired().HasMaxLength(3);
            builder.Property(v => v.Observaciones).HasMaxLength(500);
            builder.Property(v => v.IdempotencyKey).HasMaxLength(100);

            builder.Property(v => v.Subtotal).HasColumnType("decimal(18,2)");
            builder.Property(v => v.Impuesto).HasColumnType("decimal(18,2)");
            builder.Property(v => v.DescuentoTotal).HasColumnType("decimal(18,2)");
            builder.Property(v => v.Total).HasColumnType("decimal(18,2)");

            builder.Property(v => v.RowVersion)
                .IsRowVersion()
                .IsRequired();

            builder.Property(v => v.UsuarioCreacion).IsRequired().HasMaxLength(100);
            builder.Property(v => v.UsuarioModificacion).HasMaxLength(100);

            // Relaciones
            builder.HasOne(v => v.Empresa)
                .WithMany()
                .HasForeignKey(v => v.IdEmpresa)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(v => v.Sucursal)
                .WithMany()
                .HasForeignKey(v => v.IdSucursal)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(v => v.Pedido)
                .WithMany()
                .HasForeignKey(v => v.IdPedido)
                .OnDelete(DeleteBehavior.NoAction);

            // Índices / Constraints
            builder.HasIndex(v => new { v.NumeroVenta, v.IdEmpresa })
                .IsUnique();

            builder.HasIndex(v => new { v.IdPedido, v.IdEmpresa })
                .IsUnique()
                .HasFilter("[IdPedido] IS NOT NULL");

            builder.HasIndex(v => new { v.IdEmpresa, v.IdempotencyKey })
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        }
    }
}
