using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedidos", "dbo");
            builder.HasKey(p => p.IdPedido);

            builder.Property(p => p.NumeroPedido).HasMaxLength(50).IsRequired();
            builder.Property(p => p.Origen).HasMaxLength(20).IsRequired();
            builder.Property(p => p.Estado).HasMaxLength(20).IsRequired();
            builder.Property(p => p.Observaciones).HasMaxLength(500);
            builder.Property(p => p.Subtotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.Impuesto).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.DescuentoTotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.Total).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(p => p.Moneda).HasColumnType("char(3)").IsRequired();
            
            builder.Property(p => p.UsuarioProcesamiento).HasMaxLength(50);
            builder.Property(p => p.MotivoCancelacion).HasMaxLength(200);
            builder.Property(p => p.IdempotencyKey).HasMaxLength(50);
            
            builder.Property(p => p.UsuarioCreacion).HasMaxLength(50);
            builder.Property(p => p.UsuarioModificacion).HasMaxLength(50);

            builder.Property(p => p.FechaPedido).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(p => p.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");

            // Relaciones simples
            builder.HasOne(p => p.Empresa)
                   .WithMany()
                   .HasForeignKey(p => p.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Sucursal)
                   .WithMany()
                   .HasForeignKey(p => p.IdSucursal)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relaciones complejas (Claves compuestas en BD)
            builder.HasOne(p => p.Cliente)
                   .WithMany()
                   .HasForeignKey(p => new { p.IdCliente, p.IdEmpresa })
                   .HasPrincipalKey(c => new { c.IdCliente, c.IdEmpresa })
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.DireccionEntrega)
                   .WithMany()
                   .HasForeignKey(p => new { p.IdDireccionEntrega, p.IdCliente })
                   .HasPrincipalKey(cd => new { cd.IdDireccion, cd.IdCliente })
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(p => p.Detalles)
                   .WithOne(d => d.Pedido)
                   .HasForeignKey(d => d.IdPedido)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.NumeroPedido).IsUnique();
            
            // Concurrencia Optimista
            builder.Property(p => p.RowVersion)
                   .IsRowVersion()
                   .ValueGeneratedOnAddOrUpdate();
        }
    }
}
