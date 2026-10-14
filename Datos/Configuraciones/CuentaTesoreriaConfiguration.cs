using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class CuentaTesoreriaConfiguration : IEntityTypeConfiguration<CuentaTesoreria>
    {
        public void Configure(EntityTypeBuilder<CuentaTesoreria> builder)
        {
            builder.ToTable("CuentasTesoreria", "dbo");
            builder.HasKey(e => e.IdCuenta);
            builder.HasIndex(e => new { e.IdEmpresa, e.IdCuenta }).IsUnique();
            builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Activo).IsRequired();
            builder.Property(e => e.FechaCreacion).IsRequired();
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
