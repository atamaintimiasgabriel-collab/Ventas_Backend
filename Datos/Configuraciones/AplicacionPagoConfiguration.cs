using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class AplicacionPagoConfiguration : IEntityTypeConfiguration<AplicacionPago>
    {
        public void Configure(EntityTypeBuilder<AplicacionPago> builder)
        {
            builder.ToTable("AplicacionesPago", "dbo");
            builder.HasKey(e => e.IdAplicacion);
            
            builder.HasIndex(e => new { e.IdEmpresa, e.IdPago, e.IdObligacion }).IsUnique();
            
            builder.Property(e => e.MontoAplicado).HasColumnType("decimal(18,2)").IsRequired();
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.Pago)
                   .WithMany(p => p.Aplicaciones)
                   .HasPrincipalKey(p => new { p.IdEmpresa, p.IdPago })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdPago })
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.ObligacionCxC)
                   .WithMany(o => o.Aplicaciones)
                   .HasPrincipalKey(o => new { o.IdEmpresa, o.IdObligacion })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdObligacion })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
