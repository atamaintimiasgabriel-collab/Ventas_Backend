using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class SubcategoriaConfiguracion : IEntityTypeConfiguration<Subcategoria>
    {
        public void Configure(EntityTypeBuilder<Subcategoria> builder)
        {
            builder.ToTable("Subcategorias", "dbo");
            builder.HasKey(e => e.IdSubcategoria);
            builder.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Activo).HasDefaultValue(true);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            builder.HasIndex(e => new { e.IdCategoria, e.Nombre }).IsUnique();
            builder.HasIndex(e => new { e.IdCategoria, e.IdSubcategoria }).IsUnique();
            builder.HasOne(e => e.Categoria).WithMany(c => c.Subcategorias).HasForeignKey(e => e.IdCategoria).OnDelete(DeleteBehavior.Restrict);
        }
    }
}