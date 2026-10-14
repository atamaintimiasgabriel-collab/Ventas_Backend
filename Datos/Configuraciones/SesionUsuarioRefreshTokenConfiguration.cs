using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class SesionUsuarioRefreshTokenConfiguration : IEntityTypeConfiguration<SesionUsuarioRefreshToken>
    {
        public void Configure(EntityTypeBuilder<SesionUsuarioRefreshToken> builder)
        {
            builder.ToTable("SesionUsuarioRefreshTokens", "dbo", t =>
            {
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_FechaExpiracion", "[FechaExpiracion] > [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_FechaConsumido", "[FechaConsumido] IS NULL OR [FechaConsumido] >= [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_FechaRevocacion", "[FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_ReuseDetectedAt", "[ReuseDetectedAt] IS NULL OR [ReuseDetectedAt] >= [FechaCreacion]");
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_TokenHashLength", "DATALENGTH([TokenHash]) = 32");
                t.HasCheckConstraint("CK_SesionUsuarioRefreshTokens_TokenHashVersion", "[TokenHashVersion] > 0");
            });

            builder.HasKey(e => e.IdRefreshToken);

            builder.Property(e => e.TokenHash)
                .HasColumnType("varbinary(32)")
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(e => e.TokenHashVersion)
                .IsRequired()
                .HasDefaultValue((byte)1);

            builder.HasIndex(e => e.TokenHash)
                .IsUnique()
                .HasDatabaseName("UQ_SesionUsuarioRefreshTokens_TokenHash");

            builder.HasIndex(e => new { e.IdSesionUsuario, e.FechaExpiracion })
                .HasDatabaseName("IX_SesionUsuarioRefreshTokens_Sesion_Expiracion");

            builder.HasIndex(e => new { e.IdSesionUsuario, e.FechaConsumido, e.FechaRevocacion })
                .HasDatabaseName("IX_SesionUsuarioRefreshTokens_Sesion_Consumido_Revocacion");

            builder.Property(e => e.FechaCreacion)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSDATETIME()");

            builder.Property(e => e.FechaExpiracion)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(e => e.FechaConsumido)
                .HasColumnType("datetime2");

            builder.Property(e => e.FechaRevocacion)
                .HasColumnType("datetime2");

            builder.Property(e => e.MotivoRevocacion)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.ReuseDetectedAt)
                .HasColumnType("datetime2");

            builder.Property(e => e.RowVersion)
                .IsRowVersion();

            builder.HasOne(e => e.SesionUsuario)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.IdSesionUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ReplacedByToken)
                .WithMany(e => e.ReplacedTokens)
                .HasForeignKey(e => e.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
