using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ClienteDireccionConfiguracion : IEntityTypeConfiguration<ClienteDireccion>
    {
        public void Configure(EntityTypeBuilder<ClienteDireccion> builder)
        {
            builder.ToTable("ClienteDirecciones", "dbo");
            builder.HasKey(cd => cd.IdDireccion);

            builder.Property(cd => cd.TipoDireccion).HasMaxLength(20).IsRequired();
            builder.Property(cd => cd.DireccionCompleta).HasMaxLength(300).IsRequired();
            builder.Property(cd => cd.Referencia).HasMaxLength(200);
            builder.Property(cd => cd.Distrito).HasMaxLength(100);
            builder.Property(cd => cd.Provincia).HasMaxLength(100);
            builder.Property(cd => cd.Departamento).HasMaxLength(100);
            builder.Property(cd => cd.CodigoPostal).HasMaxLength(10);
            builder.Property(cd => cd.EsPrincipal).HasDefaultValue(false);
            builder.Property(cd => cd.Activo).HasDefaultValue(true);
            builder.Property(cd => cd.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(cd => cd.UsuarioCreacion).HasMaxLength(50);
        }
    }
}
