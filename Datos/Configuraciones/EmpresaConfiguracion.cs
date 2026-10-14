using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class EmpresaConfiguracion : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.ToTable("Empresas", "dbo");
            builder.HasKey(e => e.IdEmpresa);
            
            builder.Property(e => e.RazonSocial).HasMaxLength(150).IsRequired();
            builder.Property(e => e.RucDocumento).HasMaxLength(20).IsRequired();
            builder.Property(e => e.DireccionFiscal).HasMaxLength(250);
            builder.Property(e => e.Telefono).HasMaxLength(20);
            builder.Property(e => e.EmailContacto).HasMaxLength(100);
            builder.Property(e => e.NombreComercial).HasMaxLength(150);
            builder.Property(e => e.SitioWeb).HasMaxLength(200);
            
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            builder.Property(e => e.ConfiguracionFiscalJson).HasColumnType("nvarchar(max)");

            builder.HasIndex(e => e.RucDocumento).IsUnique();

            builder.HasMany(e => e.Sucursales)
                   .WithOne(s => s.Empresa)
                   .HasForeignKey(s => s.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
