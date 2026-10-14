using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones;

public sealed class UsuarioSucursalConfiguration : IEntityTypeConfiguration<UsuarioSucursal>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursal> builder)
    {
        builder.ToTable("UsuariosSucursales", "dbo");
        builder.HasKey(x => new { x.IdUsuario, x.IdSucursal });
        builder.HasIndex(x => new { x.IdUsuario, x.EsPrincipal }).HasDatabaseName("IX_UsuariosSucursales_Principal");
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.IdSucursal).OnDelete(DeleteBehavior.Restrict);
    }
}
