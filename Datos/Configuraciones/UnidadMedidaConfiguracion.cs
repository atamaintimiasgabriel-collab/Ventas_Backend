using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class UnidadMedidaConfiguracion : IEntityTypeConfiguration<UnidadMedida>
    {
        public void Configure(EntityTypeBuilder<UnidadMedida> builder)
        {
            builder.ToTable("UnidadesMedida", "dbo");
            builder.HasKey(e => e.IdUnidadMedida);
            builder.Property(e => e.Codigo).HasMaxLength(10).IsRequired();
            builder.Property(e => e.Descripcion).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.HasIndex(e => e.Codigo).IsUnique();
            builder.HasIndex(e => e.Descripcion).IsUnique();
        }
    }
}