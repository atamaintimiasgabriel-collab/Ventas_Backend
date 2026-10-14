using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class CajaConfiguration : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> builder)
        {
            builder.ToTable("Cajas", "dbo");
            builder.HasKey(e => e.IdCaja);
            builder.HasIndex(e => new { e.IdEmpresa, e.IdCaja }).IsUnique();
            builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Activo).IsRequired();
            builder.Property(e => e.FechaCreacion).IsRequired();
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            
            builder.HasOne(e => e.Empresa)
                   .WithMany()
                   .HasForeignKey(e => e.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(e => e.Sucursal)
                   .WithMany()
                   .HasPrincipalKey(s => new { s.IdEmpresa, s.IdSucursal })
                   .HasForeignKey(e => new { e.IdEmpresa, e.IdSucursal })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
