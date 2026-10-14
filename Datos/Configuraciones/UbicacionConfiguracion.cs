using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class UbicacionConfiguracion : IEntityTypeConfiguration<Ubicacion>
    {
        public void Configure(EntityTypeBuilder<Ubicacion> builder)
        {
            builder.ToTable("Ubicaciones", "dbo");
            builder.HasKey(e => e.IdUbicacion);
            builder.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Descripcion).HasMaxLength(150);
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.HasIndex(e => new { e.IdAlmacen, e.Codigo }).IsUnique();
            builder.HasIndex(e => new { e.IdUbicacion, e.IdAlmacen }).IsUnique();
            builder.HasOne(e => e.Almacen).WithMany(a => a.Ubicaciones).HasForeignKey(e => e.IdAlmacen).OnDelete(DeleteBehavior.Restrict);
        }
    }
}