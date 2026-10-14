using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class InventarioFisicoConfiguracion : IEntityTypeConfiguration<InventarioFisico>
    {
        public void Configure(EntityTypeBuilder<InventarioFisico> builder)
        {
            builder.ToTable("InventariosFisicos", "dbo");
            builder.HasKey(e => e.IdInventarioFisico);
            builder.Property(e => e.FechaProgramada).HasColumnType("date").IsRequired();
            builder.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("BORRADOR");
            builder.Property(e => e.UsuarioResponsable).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Observacion).HasMaxLength(250);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioAprobacion).HasMaxLength(50);
            
            builder.HasIndex(e => new { e.IdInventarioFisico, e.IdAlmacen }).IsUnique();
            builder.HasIndex(e => new { e.IdAlmacen, e.Estado });

            builder.HasOne(e => e.Almacen).WithMany(a => a.InventariosFisicos).HasForeignKey(e => e.IdAlmacen).OnDelete(DeleteBehavior.Restrict);
        }
    }
}