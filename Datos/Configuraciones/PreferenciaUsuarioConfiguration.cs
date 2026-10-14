using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones;

public sealed class PreferenciaUsuarioConfiguration : IEntityTypeConfiguration<PreferenciaUsuario>
{
    public void Configure(EntityTypeBuilder<PreferenciaUsuario> builder)
    {
        builder.ToTable("PreferenciasUsuario", "dbo");
        builder.HasKey(x => x.IdUsuario);
        builder.Property(x => x.Tema).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.TamanoFuente).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.DensidadInformacion).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(50).IsUnicode(false);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(50).IsUnicode(false);
        builder.HasOne(x => x.Usuario).WithOne().HasForeignKey<PreferenciaUsuario>(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
    }
}
