using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public class ComprobanteAuditoriaConfiguration : IEntityTypeConfiguration<ComprobanteAuditoria>
    {
        public void Configure(EntityTypeBuilder<ComprobanteAuditoria> builder)
        {
            builder.ToTable("ComprobantesAuditoria");
            builder.HasKey(e => e.IdAuditoria);

            builder.Property(e => e.EstadoProcesamientoAnterior).HasMaxLength(20);
            builder.Property(e => e.EstadoProcesamientoNuevo).HasMaxLength(20);
            builder.Property(e => e.EstadoFiscalAnterior).HasMaxLength(30);
            builder.Property(e => e.EstadoFiscalNuevo).HasMaxLength(30);
            builder.Property(e => e.Evento).IsRequired().HasMaxLength(100);

            builder.HasOne(e => e.Comprobante)
                .WithMany(c => c.Auditorias)
                .HasForeignKey(e => new { e.IdEmpresa, e.IdComprobante })
                .HasPrincipalKey(c => new { c.IdEmpresa, c.IdComprobante })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
