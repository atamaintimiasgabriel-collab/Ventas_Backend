using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class StockConfiguracion : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("Stock", "dbo");
            builder.HasKey(e => e.IdStock);
            builder.Property(e => e.StockFisico).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.StockReservado).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.StockDisponible).HasColumnType("decimal(18,2)").ValueGeneratedOnAddOrUpdate();
            builder.Property(e => e.StockMinimo).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.StockMaximo).HasColumnType("decimal(18,2)");
            builder.Property(e => e.StockSeguridad).HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            builder.Property(e => e.Lote).HasMaxLength(50);
            builder.Property(e => e.FechaVencimiento).HasColumnType("date");
            builder.Property(e => e.FechaUltimaActualizacion).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioUltimaActualizacion).HasMaxLength(50);
            
            builder.Property(e => e.RowVersion).IsRowVersion();
            
            builder.HasIndex(e => new { e.IdProducto, e.IdAlmacen, e.IdUbicacion }).IsUnique();
            
            builder.HasOne(e => e.Producto).WithMany(p => p.Stocks).HasForeignKey(e => e.IdProducto).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Ubicacion).WithMany(u => u.Stocks).HasForeignKey(e => new { e.IdUbicacion, e.IdAlmacen }).HasPrincipalKey(u => new { u.IdUbicacion, u.IdAlmacen }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}