using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
    {
        public void Configure(EntityTypeBuilder<Permiso> builder)
        {
            builder.ToTable("Permisos", "dbo");

            builder.HasKey(e => e.IdPermiso);

            builder.Property(e => e.CodigoSistema)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode(false);

            builder.HasIndex(e => e.CodigoSistema)
                .IsUnique()
                .HasDatabaseName("UQ_Permisos_CodigoSistema");

            builder.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsRequired()
                .IsUnicode(false);

            builder.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsRequired(false)
                .IsUnicode(false);

            builder.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            // Auditoría
            builder.Property(e => e.FechaCreacion)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            builder.Property(e => e.UsuarioCreacion)
                .HasMaxLength(50)
                .IsRequired(false)
                .IsUnicode(false);

            builder.Property(e => e.FechaModificacion)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(e => e.UsuarioModificacion)
                .HasMaxLength(50)
                .IsRequired(false)
                .IsUnicode(false);
        }
    }
}
