using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class SucursalConfiguracion : IEntityTypeConfiguration<Sucursal>
    {
        public void Configure(EntityTypeBuilder<Sucursal> builder)
        {
            builder.ToTable("Sucursales", "dbo");
            builder.HasKey(s => s.IdSucursal);
            
            builder.Property(s => s.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(s => s.CodigoEstablecimiento).HasMaxLength(10);
            builder.Property(s => s.Direccion).HasMaxLength(250);
            builder.Property(s => s.Telefono).HasMaxLength(20);
            
            builder.Property(s => s.Activo).HasDefaultValue(true);
            builder.Property(s => s.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(s => s.UsuarioCreacion).HasMaxLength(50);
            builder.Property(s => s.UsuarioModificacion).HasMaxLength(50);

            // Índice único para evitar sucursales con el mismo nombre en la misma empresa
            builder.HasIndex(s => new { s.IdEmpresa, s.Nombre }).IsUnique();

            builder.HasMany(s => s.Almacenes)
                   .WithOne(a => a.Sucursal)
                   .HasForeignKey(a => a.IdSucursal)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
