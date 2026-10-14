using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
    {
        public void Configure(EntityTypeBuilder<Comprobante> builder)
        {
            builder.ToTable("Comprobantes");
            builder.HasKey(e => e.IdComprobante);

            builder.Property(e => e.TipoComprobante).IsRequired().HasMaxLength(20);
            builder.Property(e => e.SerieFiscal).IsRequired().HasMaxLength(4);
            builder.Property(e => e.VersionNormativa).HasMaxLength(40);
            builder.Property(e => e.TipoOperacionSunat).HasMaxLength(4);
            builder.Property(e => e.TipoDocumentoOrigen).HasMaxLength(2);
            builder.Property(e => e.SerieDocumentoOrigen).HasMaxLength(4);
            builder.Property(e => e.CodigoMotivoNota).HasMaxLength(2);
            builder.Property(e => e.SustentoNota).HasMaxLength(500);
            
            builder.Property(e => e.EstadoProcesamiento).IsRequired().HasMaxLength(20);
            builder.Property(e => e.WorkerId).HasMaxLength(100);
            builder.Property(e => e.EstadoFiscal).IsRequired().HasMaxLength(30);
            
            builder.Property(e => e.IdOperacionExterna).HasMaxLength(100);
            builder.Property(e => e.IdDocumentoExterno).HasMaxLength(100);
            
            builder.Property(e => e.StorageKeyXml).HasMaxLength(255);
            builder.Property(e => e.StorageKeyCdr).HasMaxLength(255);
            builder.Property(e => e.StorageKeyPdf).HasMaxLength(255);
            
            builder.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            builder.Property(e => e.UsuarioModificacion).HasMaxLength(50);
            
            builder.Property(e => e.RowVersion).IsRowVersion();

            builder.HasIndex(e => new { e.IdEmpresa, e.IdComprobante })
                .IsUnique()
                .HasDatabaseName("UQ_Comprobantes_IdEmpresa_IdComprobante");

            builder.HasIndex(e => new { e.IdEmpresa, e.TipoComprobante, e.SerieFiscal, e.CorrelativoFiscal })
                .IsUnique()
                .HasDatabaseName("UQ_Comprobantes_IdentidadFiscal")
                .HasFilter("[CorrelativoFiscal] IS NOT NULL");

            builder.HasIndex(e => new { e.EstadoProcesamiento, e.LeaseHasta, e.FechaProximoIntento })
                .HasDatabaseName("IX_Comprobantes_WorkerEmisor")
                .HasFilter("[EstadoFiscal] = 'NO_ENVIADO'");

            builder.HasIndex(e => new { e.EstadoFiscal, e.FechaProximoIntento })
                .HasDatabaseName("IX_Comprobantes_WorkerConsultor")
                .HasFilter("[EstadoFiscal] = 'ENVIADO_DESCONOCIDO'");

            builder.HasIndex(e => new { e.IdEmpresa, e.IdVenta })
                .HasDatabaseName("IX_Comprobantes_Venta");

            builder.HasIndex(e => new { e.IdEmpresa, e.IdComprobanteOrigen })
                .HasDatabaseName("IX_Comprobantes_Origen");

            builder.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Sucursal)
                .WithMany()
                .HasForeignKey(e => new { e.IdEmpresa, e.IdSucursal })
                .HasPrincipalKey(s => new { s.IdEmpresa, s.IdSucursal })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Venta)
                .WithMany()
                .HasForeignKey(e => new { e.IdEmpresa, e.IdVenta })
                .HasPrincipalKey(v => new { v.IdEmpresa, v.IdVenta })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ComprobanteOrigen)
                .WithMany()
                .HasForeignKey(e => new { e.IdEmpresa, e.IdComprobanteOrigen })
                .HasPrincipalKey(c => new { c.IdEmpresa, c.IdComprobante })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
