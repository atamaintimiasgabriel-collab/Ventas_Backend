using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class CanalPagoConfiguration : IEntityTypeConfiguration<CanalPago>
    {
        public void Configure(EntityTypeBuilder<CanalPago> builder)
        {
            builder.ToTable("CanalesPago", "dbo");
            builder.HasKey(e => e.IdCanal);
            builder.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Activo).IsRequired();
            builder.Property(e => e.FechaCreacion).IsRequired();
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
        }
    }
}
