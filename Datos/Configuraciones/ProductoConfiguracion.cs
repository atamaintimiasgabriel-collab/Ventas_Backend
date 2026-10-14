using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos", "dbo");
            builder.HasKey(e => e.IdProducto);
            builder.Property(e => e.SKU).HasMaxLength(50).IsRequired();
            builder.Property(e => e.CodigoBarras).HasMaxLength(100);
            builder.Property(e => e.CodigoOEM).HasMaxLength(100);
            builder.Property(e => e.NombreComercial).HasMaxLength(200).IsRequired();
            builder.Property(e => e.DescripcionTecnica).HasColumnType("varchar(max)");
            builder.Property(e => e.PrecioCompra).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.PrecioVenta).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.MargenGanancia).HasColumnType("decimal(5,2)");
            builder.Property(e => e.PrecioOferta).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Activo");
            builder.Property(e => e.RutaImagen).HasMaxLength(500);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            builder.Property(e => e.ClasificacionTributaria).HasMaxLength(10).HasDefaultValue("GRA");
            
            builder.HasIndex(e => e.SKU).IsUnique();
            builder.HasIndex(e => e.CodigoBarras);
            builder.HasIndex(e => e.CodigoOEM);
            builder.HasIndex(e => e.NombreComercial);

            builder.HasOne(e => e.Categoria).WithMany(c => c.Productos).HasForeignKey(e => e.IdCategoria).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Marca).WithMany(m => m.Productos).HasForeignKey(e => e.IdMarca).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.UnidadMedida).WithMany(u => u.Productos).HasForeignKey(e => e.IdUnidadMedida).OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Subcategoria).WithMany(s => s.Productos).HasForeignKey(e => new { e.IdCategoria, e.IdSubcategoria }).HasPrincipalKey(s => new { s.IdCategoria, s.IdSubcategoria }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}