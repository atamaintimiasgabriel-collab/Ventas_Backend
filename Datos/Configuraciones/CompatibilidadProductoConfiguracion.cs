using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class CompatibilidadProductoConfiguracion : IEntityTypeConfiguration<CompatibilidadProducto>
    {
        public void Configure(EntityTypeBuilder<CompatibilidadProducto> builder)
        {
            builder.ToTable("CompatibilidadProducto", "dbo");
            builder.HasKey(e => e.IdCompatibilidad);
            builder.Property(e => e.Observacion).HasMaxLength(200);
            builder.HasIndex(e => new { e.IdProducto, e.IdVersionVehiculo }).IsUnique();
            builder.HasOne(e => e.Producto).WithMany(p => p.Compatibilidades).HasForeignKey(e => e.IdProducto).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.VersionVehiculo).WithMany(v => v.Compatibilidades).HasForeignKey(e => e.IdVersionVehiculo).OnDelete(DeleteBehavior.Restrict);
        }
    }
}