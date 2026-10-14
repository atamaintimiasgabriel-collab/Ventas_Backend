using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    // Configuración de la entidad pivote RolPermiso
    public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
    {
        public void Configure(EntityTypeBuilder<RolPermiso> builder)
        {
            builder.ToTable("RolesPermisos", "dbo");

            // PK Compuesta definida en la base de datos
            builder.HasKey(e => new { e.IdRol, e.IdPermiso });

            // Relación con Rol
            // Mantiene el DeleteBehavior.Restrict para asegurar NO ACTION.
            builder.HasOne(rp => rp.Rol)
                .WithMany(r => r.RolesPermisos)
                .HasForeignKey(rp => rp.IdRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RolesPermisos_Roles");

            // Relación con Permiso
            builder.HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolesPermisos)
                .HasForeignKey(rp => rp.IdPermiso)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_RolesPermisos_Permisos");

            // Índice inverso para búsquedas rápidas (IdPermiso)
            builder.HasIndex(rp => rp.IdPermiso)
                .HasDatabaseName("IX_RolesPermisos_IdPermiso");
        }
    }
}
