using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    // Configura la entidad Usuario asegurando que EF Core respete el modelo SQL-FIRST
    // sin intentar generar esquemas automáticos o cascadas no deseadas.
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", "dbo");

            builder.HasKey(e => e.IdUsuario);

            builder.Property(e => e.Username)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode(false); // VARCHAR en lugar de NVARCHAR

            builder.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("UQ_Usuarios_Username");

            builder.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired()
                .IsUnicode(false);

            builder.Property(e => e.Email)
                .HasMaxLength(100)
                .IsRequired()
                .IsUnicode(false);

            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Usuarios_Email");

            builder.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            // Campos de auditoría
            builder.Property(e => e.FechaCreacion)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            builder.Property(e => e.UsuarioCreacion)
                .HasMaxLength(50)
                .IsRequired(false) // Permite NULL por la estrategia de bootstrap inicial
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
