using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public class SerieFiscalConfiguration : IEntityTypeConfiguration<SerieFiscal>
    {
        public void Configure(EntityTypeBuilder<SerieFiscal> builder)
        {
            builder.ToTable("SeriesFiscales");
            builder.HasKey(e => e.IdSerieFiscal);

            builder.Property(e => e.TipoComprobante).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Serie).IsRequired().HasMaxLength(4);
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);

            builder.HasIndex(e => new { e.IdEmpresa, e.TipoComprobante, e.Serie })
                .IsUnique()
                .HasDatabaseName("UQ_SeriesFiscales_Identidad");

            builder.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Sucursal)
                .WithMany()
                .HasForeignKey(e => new { e.IdEmpresa, e.IdSucursal })
                .HasPrincipalKey(s => new { s.IdEmpresa, s.IdSucursal })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
