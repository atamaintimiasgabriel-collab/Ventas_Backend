using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ModeloVehiculoConfiguracion : IEntityTypeConfiguration<ModeloVehiculo>
    {
        public void Configure(EntityTypeBuilder<ModeloVehiculo> builder)
        {
            builder.ToTable("ModelosVehiculos", "dbo");
            builder.HasKey(e => e.IdModeloVehiculo);
            builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Cilindrada).HasMaxLength(20);
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.HasIndex(e => new { e.IdMarcaVehiculo, e.Nombre }).IsUnique();
            builder.HasOne(e => e.MarcaVehiculo).WithMany(m => m.Modelos).HasForeignKey(e => e.IdMarcaVehiculo).OnDelete(DeleteBehavior.Restrict);
        }
    }
}