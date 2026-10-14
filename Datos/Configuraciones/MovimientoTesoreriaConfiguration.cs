using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class MovimientoTesoreriaConfiguration : IEntityTypeConfiguration<MovimientoTesoreria>
    {
        public void Configure(EntityTypeBuilder<MovimientoTesoreria> builder)
        {
            builder.ToTable("MovimientosTesoreria", "dbo");
            builder.HasKey(e => e.IdMovTesoreria);
            
            builder.Property(e => e.Tipo).IsRequired().HasMaxLength(10);
            builder.Property(e => e.Monto).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.IdOrigen).HasMaxLength(100);
            builder.Property(e => e.ReferenciaExterna).HasMaxLength(100);
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.CuentaTesoreria)
                   .WithMany()
                   .HasPrincipalKey(c => new { c.IdEmpresa, c.IdCuenta })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdCuenta })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
