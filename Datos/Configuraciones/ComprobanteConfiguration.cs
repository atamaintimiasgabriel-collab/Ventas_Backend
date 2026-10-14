using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    /// <summary>
    /// Configuración EF Core para Comprobante.
    /// Objetivo crítico: registrar RowVersion como token de concurrencia optimista
    /// para que DbUpdateConcurrencyException se lance cuando dos workers compiten.
    /// Las relaciones de navegación se dejan a la convención EF (nombres IdXxx).
    /// </summary>
    public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
    {
        public void Configure(EntityTypeBuilder<Comprobante> builder)
        {
            builder.ToTable("Comprobantes");

            builder.HasKey(c => c.IdComprobante);

            // GUID generado en la aplicación
            builder.Property(c => c.IdComprobante)
                   .ValueGeneratedNever();

            builder.Property(c => c.TipoComprobante)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.SerieFiscal)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(c => c.EstadoProcesamiento)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.WorkerId)
                   .HasMaxLength(100);

            builder.Property(c => c.EstadoFiscal)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.IdOperacionExterna)
                   .HasMaxLength(200);

            builder.Property(c => c.IdDocumentoExterno)
                   .HasMaxLength(200);

            builder.Property(c => c.StorageKeyXml)
                   .HasMaxLength(500);

            builder.Property(c => c.StorageKeyCdr)
                   .HasMaxLength(500);

            builder.Property(c => c.StorageKeyPdf)
                   .HasMaxLength(500);

            builder.Property(c => c.FechaCreacion)
                   .IsRequired();

            builder.Property(c => c.UsuarioCreacion)
                   .HasMaxLength(100);

            builder.Property(c => c.UsuarioModificacion)
                   .HasMaxLength(100);

            // RowVersion: token de concurrencia optimista (SQL Server timestamp/rowversion)
            // CRÍTICO: sin esto EF no lanza DbUpdateConcurrencyException al detectar conflictos
            builder.Property(c => c.RowVersion)
                   .IsRowVersion()
                   .IsConcurrencyToken();

            // Las FKs (IdEmpresa, IdSucursal, IdVenta, IdComprobanteOrigen) son inferidas
            // por convención de nombres. No las redefinimos aquí para evitar conflictos
            // de compatibilidad de tipos nullable vs non-nullable.

            // Auditorías (relación 1:N — el lado N está en ComprobanteAuditoria)
            builder.HasMany(c => c.Auditorias)
                   .WithOne(a => a.Comprobante)
                   .HasForeignKey(a => a.IdComprobante)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Configuración EF Core para ComprobanteAuditoria.
    /// Inmutable por diseño: solo se insertan, nunca se modifican.
    /// </summary>
    public class ComprobanteAuditoriaConfiguration : IEntityTypeConfiguration<ComprobanteAuditoria>
    {
        public void Configure(EntityTypeBuilder<ComprobanteAuditoria> builder)
        {
            builder.ToTable("ComprobantesAuditoria");

            builder.HasKey(a => a.IdAuditoria);

            builder.Property(a => a.IdAuditoria)
                   .ValueGeneratedOnAdd();

            builder.Property(a => a.Evento)
                   .IsRequired()
                   .HasMaxLength(60);

            builder.Property(a => a.EstadoProcesamientoAnterior)
                   .HasMaxLength(30);

            builder.Property(a => a.EstadoProcesamientoNuevo)
                   .HasMaxLength(30);

            builder.Property(a => a.EstadoFiscalAnterior)
                   .HasMaxLength(30);

            builder.Property(a => a.EstadoFiscalNuevo)
                   .HasMaxLength(30);

            builder.Property(a => a.Detalle)
                   .HasMaxLength(1000);

            builder.Property(a => a.FechaEvento)
                   .IsRequired();

            // La relación inversa (HasOne/WithMany) ya está definida en ComprobanteConfiguration
        }
    }
}
