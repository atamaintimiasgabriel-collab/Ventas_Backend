using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ObligacionCxCConfiguration : IEntityTypeConfiguration<ObligacionCxC>
    {
        public void Configure(EntityTypeBuilder<ObligacionCxC> builder)
        {
            builder.ToTable("ObligacionesCxC", "dbo");
            builder.HasKey(e => e.IdObligacion);
            
            builder.HasIndex(e => new { e.IdEmpresa, e.IdObligacion }).IsUnique();
            builder.HasIndex(e => new { e.IdEmpresa, e.IdVentaOrigen })
                   .IsUnique()
                   .HasFilter("[IdVentaOrigen] IS NOT NULL");
            
            builder.Property(e => e.Moneda).IsRequired().HasColumnType("char(3)").HasMaxLength(3);
            builder.Property(e => e.DeudaOriginal).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.Estado).IsRequired().HasMaxLength(20);
            
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
                   
            builder.HasOne(e => e.Venta)
                   .WithMany()
                   .HasPrincipalKey(v => new { v.IdEmpresa, v.IdVenta })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdVentaOrigen })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
