using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class AperturaCajaConfiguration : IEntityTypeConfiguration<AperturaCaja>
    {
        public void Configure(EntityTypeBuilder<AperturaCaja> builder)
        {
            builder.ToTable("AperturasCajas", "dbo");
            builder.HasKey(e => e.IdApertura);
            
            builder.HasIndex(e => new { e.IdEmpresa, e.IdCaja })
                   .IsUnique()
                   .HasFilter("[Estado] = 'ABIERTA'");
                   
            builder.Property(e => e.SaldoInicial).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.SaldoEsperado).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.SaldoContado).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Estado).IsRequired().HasMaxLength(20);
            builder.Property(e => e.RowVersion).IsRowVersion();
            
            builder.HasOne(e => e.Caja)
                   .WithMany(c => c.AperturasCajas)
                   .HasPrincipalKey(c => new { c.IdEmpresa, c.IdCaja })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdCaja })
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.Usuario)
                   .WithMany()
                   .HasForeignKey(e => e.IdUsuario)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
