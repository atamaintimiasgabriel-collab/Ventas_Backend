using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
    {
        public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
        {
            builder.ToTable("MovimientosCajas", "dbo");
            builder.HasKey(e => e.IdMovCaja);
            
            builder.Property(e => e.Tipo).IsRequired().HasMaxLength(10);
            builder.Property(e => e.Monto).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.IdOrigen).HasMaxLength(100);
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.AperturaCaja)
                   .WithMany(a => a.Movimientos)
                   .HasForeignKey(e => e.IdApertura)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
