using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaVentasMotorrepuestos.API.Modelos;

namespace SistemaVentasMotorrepuestos.API.Datos.Configuraciones
{
    public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes", "dbo");
            builder.HasKey(c => c.IdCliente);

            builder.Property(c => c.TipoPersona).HasColumnType("char(1)").IsRequired();
            builder.Property(c => c.TipoDocumento).HasMaxLength(10).IsRequired();
            builder.Property(c => c.NumeroDocumento).HasMaxLength(20).IsRequired();
            builder.Property(c => c.RazonSocial).HasMaxLength(200);
            builder.Property(c => c.NombreComercial).HasMaxLength(200);
            builder.Property(c => c.Nombres).HasMaxLength(100);
            builder.Property(c => c.Apellidos).HasMaxLength(100);
            builder.Property(c => c.Email).HasMaxLength(100);
            builder.Property(c => c.Telefono).HasMaxLength(20);
            builder.Property(c => c.LimiteCredito).HasColumnType("decimal(18,2)");

            builder.Property(c => c.Activo).HasDefaultValue(true);
            builder.Property(c => c.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            builder.Property(c => c.UsuarioCreacion).HasMaxLength(50);
            builder.Property(c => c.UsuarioModificacion).HasMaxLength(50);

            // Unicidad: (IdEmpresa, TipoDocumento, NumeroDocumento)
            builder.HasIndex(c => new { c.IdEmpresa, c.TipoDocumento, c.NumeroDocumento }).IsUnique();

            // Índice filtrado para Email (EF Core no soporta WHERE en HasIndex, se creó en DDL)

            // FK Empresa → Cliente
            builder.HasOne(c => c.Empresa)
                   .WithMany()
                   .HasForeignKey(c => c.IdEmpresa)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relaciones hijas
            builder.HasMany(c => c.Contactos)
                   .WithOne(cc => cc.Cliente)
                   .HasForeignKey(cc => cc.IdCliente)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Direcciones)
                   .WithOne(cd => cd.Cliente)
                   .HasForeignKey(cd => cd.IdCliente)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
