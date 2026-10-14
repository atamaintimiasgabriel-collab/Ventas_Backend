using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class VersionVehiculoConfiguracion : IEntityTypeConfiguration<VersionVehiculo>
    {
        public void Configure(EntityTypeBuilder<VersionVehiculo> builder)
        {
            builder.ToTable("VersionesVehiculos", "dbo");
            builder.HasKey(e => e.IdVersionVehiculo);
            builder.Property(e => e.ConfiguracionMotor).HasMaxLength(100);
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.HasOne(e => e.ModeloVehiculo).WithMany(m => m.Versiones).HasForeignKey(e => e.IdModeloVehiculo).OnDelete(DeleteBehavior.Restrict);
        }
    }
}