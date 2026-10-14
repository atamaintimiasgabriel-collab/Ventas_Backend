using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ClienteContactoConfiguracion : IEntityTypeConfiguration<ClienteContacto>
    {
        public void Configure(EntityTypeBuilder<ClienteContacto> builder)
        {
            builder.ToTable("ClienteContactos", "dbo");
            builder.HasKey(cc => cc.IdContacto);

            builder.Property(cc => cc.TipoContacto).HasMaxLength(20).IsRequired();
            builder.Property(cc => cc.Valor).HasMaxLength(150).IsRequired();
            builder.Property(cc => cc.EsPrincipal).HasDefaultValue(false);
            builder.Property(cc => cc.Observacion).HasMaxLength(200);
            builder.Property(cc => cc.Activo).HasDefaultValue(true);
            builder.Property(cc => cc.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(cc => cc.UsuarioCreacion).HasMaxLength(50);
        }
    }
}
