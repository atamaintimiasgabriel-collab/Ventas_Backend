using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    // Configuración de la entidad pivote UsuarioRol
    // Aplica PK compuesta y restringe las eliminaciones en cascada.
    public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
    {
        public void Configure(EntityTypeBuilder<UsuarioRol> builder)
        {
            builder.ToTable("UsuariosRoles", "dbo");

            // PK Compuesta definida en la base de datos
            builder.HasKey(e => new { e.IdUsuario, e.IdRol });

            // Relación con Usuario. 
            // OnDelete(DeleteBehavior.Restrict) genera comportamiento NO ACTION
            // que previene el borrado en cascada (destrucción de historial).
            builder.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuariosRoles)
                .HasForeignKey(ur => ur.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UsuariosRoles_Usuarios");

            // Relación con Rol
            builder.HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuariosRoles)
                .HasForeignKey(ur => ur.IdRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UsuariosRoles_Roles");

            // Índice inverso para búsquedas rápidas (IdRol)
            builder.HasIndex(ur => ur.IdRol)
                .HasDatabaseName("IX_UsuariosRoles_IdRol");
        }
    }
}
