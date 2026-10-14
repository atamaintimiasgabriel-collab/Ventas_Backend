using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class MovimientoCxCConfiguration : IEntityTypeConfiguration<MovimientoCxC>
    {
        public void Configure(EntityTypeBuilder<MovimientoCxC> builder)
        {
            builder.ToTable("MovimientosCxC", "dbo");
            builder.HasKey(e => e.IdMovCxC);
            
            builder.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Monto).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.IdReferenciaOrigen).HasMaxLength(100);
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.ObligacionCxC)
                   .WithMany(o => o.Movimientos)
                   .HasPrincipalKey(o => new { o.IdEmpresa, o.IdObligacion })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdObligacion })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
