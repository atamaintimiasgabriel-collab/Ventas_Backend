using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("Roles", "dbo");

            builder.HasKey(e => e.IdRol);

            builder.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode(false);

            builder.HasIndex(e => e.NombreRol)
                .IsUnique()
                .HasDatabaseName("UQ_Roles_NombreRol");

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
