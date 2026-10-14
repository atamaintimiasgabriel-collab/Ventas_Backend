using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class TipoMovimientoConfiguracion : IEntityTypeConfiguration<TipoMovimiento>
    {
        public void Configure(EntityTypeBuilder<TipoMovimiento> builder)
        {
            builder.ToTable("TiposMovimiento", "dbo");
            builder.HasKey(e => e.IdTipoMovimiento);
            builder.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.HasIndex(e => e.Codigo).IsUnique();
        }
    }
}