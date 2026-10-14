using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class MarcaVehiculoConfiguracion : IEntityTypeConfiguration<MarcaVehiculo>
    {
        public void Configure(EntityTypeBuilder<MarcaVehiculo> builder)
        {
            builder.ToTable("MarcasVehiculos", "dbo");
            builder.HasKey(e => e.IdMarcaVehiculo);
            builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.HasIndex(e => e.Nombre).IsUnique();
        }
    }
}