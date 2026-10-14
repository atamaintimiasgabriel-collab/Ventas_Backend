using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ProveedorPagoConfiguration : IEntityTypeConfiguration<ProveedorPago>
    {
        public void Configure(EntityTypeBuilder<ProveedorPago> builder)
        {
            builder.ToTable("ProveedoresPago", "dbo");
            builder.HasKey(e => e.IdProveedor);
            builder.HasIndex(e => new { e.IdEmpresa, e.IdProveedor }).IsUnique();
            builder.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
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
