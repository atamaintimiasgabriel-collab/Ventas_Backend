using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class SesionUsuarioConfiguration : IEntityTypeConfiguration<SesionUsuario>
    {
        public void Configure(EntityTypeBuilder<SesionUsuario> builder)
        {
            builder.ToTable("SesionesUsuarios", "dbo", t =>
            {
                t.HasCheckConstraint("CK_SesionesUsuarios_FechaExpiracion", "[FechaExpiracion] > [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionesUsuarios_FechaRevocacion", "[FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionesUsuarios_FechaUltimaActividad", "[FechaUltimaActividad] IS NULL OR [FechaUltimaActividad] >= [FechaCreacion]");
            });

            builder.HasKey(e => e.IdSesionUsuario);

            builder.Property(e => e.SessionId)
                .IsRequired();

            builder.HasIndex(e => e.SessionId)
                .IsUnique()
                .HasDatabaseName("UQ_SesionesUsuarios_SessionId");

            builder.HasIndex(e => new { e.IdUsuario, e.FechaRevocacion, e.FechaExpiracion })
                .HasDatabaseName("IX_SesionesUsuarios_Usuario_Revocacion_Expiracion");

            builder.HasIndex(e => e.FechaExpiracion)
                .HasDatabaseName("IX_SesionesUsuarios_FechaExpiracion");

            builder.Property(e => e.FechaCreacion)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            builder.Property(e => e.FechaExpiracion)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(e => e.FechaUltimaActividad)
                .HasColumnType("datetime2");

            builder.Property(e => e.FechaRevocacion)
                .HasColumnType("datetime2");

            builder.Property(e => e.MotivoRevocacion)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.RevocadaPor)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.RowVersion)
                .IsRowVersion();

            builder.HasOne(e => e.Usuario)
                .WithMany(e => e.SesionesUsuarios)
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
