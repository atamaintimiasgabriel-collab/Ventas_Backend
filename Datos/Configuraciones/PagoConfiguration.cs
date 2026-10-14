using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos", "dbo");
            builder.HasKey(e => e.IdPago);
            
            builder.HasIndex(e => new { e.IdEmpresa, e.IdPago }).IsUnique();
            builder.HasIndex(e => new { e.IdEmpresa, e.IdempotencyKey })
                   .IsUnique()
                   .HasFilter("[IdempotencyKey] IS NOT NULL");
                   
            builder.HasIndex(e => new { e.IdEmpresa, e.IdProveedor, e.ReferenciaExterna })
                   .IsUnique()
                   .HasFilter("[IdProveedor] IS NOT NULL AND [ReferenciaExterna] IS NOT NULL");
                   
            builder.Property(e => e.MontoTotal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.Moneda).IsRequired().HasColumnType("char(3)").HasMaxLength(3);
            builder.Property(e => e.Estado).IsRequired().HasMaxLength(20);
            builder.Property(e => e.ReferenciaExterna).HasMaxLength(100);
            builder.Property(e => e.IdempotencyKey).HasMaxLength(100);
            builder.Property(e => e.CodigoAutorizacion).HasMaxLength(100);
            builder.Property(e => e.Lote).HasMaxLength(100);
            builder.Property(e => e.Ultimos4Digitos).HasMaxLength(4);
            builder.Property(e => e.Adquirente).HasMaxLength(100);
            
            builder.Property(e => e.RowVersion).IsRowVersion();
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.Cliente)
                   .WithMany()
                   .HasPrincipalKey(c => new { c.IdEmpresa, c.IdCliente })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdCliente })
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.MedioPago)
                   .WithMany()
                   .HasForeignKey(e => e.IdMedioPago)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.CanalPago)
                   .WithMany()
                   .HasForeignKey(e => e.IdCanalPago)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.ProveedorPago)
                   .WithMany()
                   .HasPrincipalKey(p => new { p.IdEmpresa, p.IdProveedor })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdProveedor })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
