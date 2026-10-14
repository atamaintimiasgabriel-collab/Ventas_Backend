using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class DetalleTransferenciaConfiguracion : IEntityTypeConfiguration<DetalleTransferencia>
    {
        public void Configure(EntityTypeBuilder<DetalleTransferencia> builder)
        {
            builder.ToTable("DetalleTransferencia", "dbo");
            builder.HasKey(e => e.IdDetalleTransferencia);
            builder.Property(e => e.Cantidad).HasColumnType("decimal(18,2)").IsRequired();
            
            builder.HasIndex(e => new { e.IdTransferencia, e.IdProducto, e.IdUbicacionOrigen, e.IdUbicacionDestino }).IsUnique();

            builder.HasOne(e => e.Transferencia).WithMany(t => t.Detalles).HasForeignKey(e => new { e.IdTransferencia, e.IdAlmacenOrigen, e.IdAlmacenDestino }).HasPrincipalKey(t => new { t.IdTransferencia, t.IdAlmacenOrigen, t.IdAlmacenDestino }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Producto).WithMany(p => p.DetallesTransferencia).HasForeignKey(e => e.IdProducto).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.UbicacionOrigen).WithMany(u => u.DetallesTransferenciaOrigen).HasForeignKey(e => new { e.IdUbicacionOrigen, e.IdAlmacenOrigen }).HasPrincipalKey(u => new { u.IdUbicacion, u.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.UbicacionDestino).WithMany(u => u.DetallesTransferenciaDestino).HasForeignKey(e => new { e.IdUbicacionDestino, e.IdAlmacenDestino }).HasPrincipalKey(u => new { u.IdUbicacion, u.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}