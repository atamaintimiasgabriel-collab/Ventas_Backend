using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class TransferenciaConfiguracion : IEntityTypeConfiguration<Transferencia>
    {
        public void Configure(EntityTypeBuilder<Transferencia> builder)
        {
            builder.ToTable("Transferencias", "dbo");
            builder.HasKey(e => e.IdTransferencia);
            builder.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("PENDIENTE");
            builder.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.UsuarioRegistro).HasMaxLength(50).IsRequired();
            builder.Property(e => e.UsuarioConfirmacion).HasMaxLength(50);
            builder.Property(e => e.Observacion).HasMaxLength(250);
            
            builder.HasIndex(e => new { e.IdTransferencia, e.IdAlmacenOrigen, e.IdAlmacenDestino }).IsUnique();

            builder.HasOne(e => e.AlmacenOrigen).WithMany(a => a.TransferenciasOrigen).HasForeignKey(e => e.IdAlmacenOrigen).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.AlmacenDestino).WithMany(a => a.TransferenciasDestino).HasForeignKey(e => e.IdAlmacenDestino).OnDelete(DeleteBehavior.Restrict);
        }
    }
}