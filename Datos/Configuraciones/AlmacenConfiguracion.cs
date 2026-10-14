using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class AlmacenConfiguracion : IEntityTypeConfiguration<Almacen>
    {
        public void Configure(EntityTypeBuilder<Almacen> builder)
        {
            builder.ToTable("Almacenes", "dbo");
            builder.HasKey(e => e.IdAlmacen);
            builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Direccion).HasMaxLength(250);
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            
            builder.Property(e => e.IdSucursal).IsRequired(false); // NULLABLE inicialmente para migración segura

            builder.HasIndex(e => e.Nombre).IsUnique();

            builder.HasOne(a => a.Sucursal)
                   .WithMany(s => s.Almacenes)
                   .HasForeignKey(a => a.IdSucursal)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}