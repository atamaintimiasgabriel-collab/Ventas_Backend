using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ProductoEquivalenteConfiguracion : IEntityTypeConfiguration<ProductoEquivalente>
    {
        public void Configure(EntityTypeBuilder<ProductoEquivalente> builder)
        {
            builder.ToTable("ProductosEquivalentes", "dbo");
            builder.HasKey(e => new { e.IdProductoPrincipal, e.IdProductoSustituto });
            builder.HasOne(e => e.ProductoPrincipal).WithMany(p => p.EquivalentesSustitutos).HasForeignKey(e => e.IdProductoPrincipal).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ProductoSustituto).WithMany(p => p.EquivalentesPrincipales).HasForeignKey(e => e.IdProductoSustituto).OnDelete(DeleteBehavior.Restrict);
        }
    }
}