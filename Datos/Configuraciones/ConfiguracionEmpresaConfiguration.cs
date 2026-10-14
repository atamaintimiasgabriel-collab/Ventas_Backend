using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones;

public sealed class ConfiguracionEmpresaConfiguration : IEntityTypeConfiguration<ConfiguracionEmpresa>
{
    public void Configure(EntityTypeBuilder<ConfiguracionEmpresa> builder)
    {
        builder.ToTable("ConfiguracionesEmpresa", "dbo");
        builder.HasKey(x => x.IdEmpresa);
        builder.Property(x => x.Moneda).HasMaxLength(3).IsUnicode(false).IsRequired();
        builder.Property(x => x.Idioma).HasMaxLength(10).IsUnicode(false).IsRequired();
        builder.Property(x => x.ZonaHoraria).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(x => x.FormatoFecha).HasMaxLength(32).IsUnicode(false).IsRequired();
        builder.Property(x => x.FormatoHora).HasMaxLength(32).IsUnicode(false).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(50).IsUnicode(false);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(50).IsUnicode(false);
        builder.HasOne(x => x.Empresa).WithOne().HasForeignKey<ConfiguracionEmpresa>(x => x.IdEmpresa).OnDelete(DeleteBehavior.Restrict);
    }
}
