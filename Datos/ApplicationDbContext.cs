using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Datos
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Subcategoria> Subcategorias { get; set; } = null!;
        public DbSet<Marca> Marcas { get; set; } = null!;
        public DbSet<UnidadMedida> UnidadesMedida { get; set; } = null!;
        public DbSet<Proveedor> Proveedores { get; set; } = null!;
        public DbSet<Compra> Compras { get; set; } = null!;
        public DbSet<CompraDetalle> CompraDetalles { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<MarcaVehiculo> MarcasVehiculos { get; set; } = null!;
        public DbSet<ModeloVehiculo> ModelosVehiculos { get; set; } = null!;
        public DbSet<VersionVehiculo> VersionesVehiculos { get; set; } = null!;
        public DbSet<CompatibilidadProducto> CompatibilidadesProducto { get; set; } = null!;
        public DbSet<ProductoEquivalente> ProductosEquivalentes { get; set; } = null!;
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Sucursal> Sucursales { get; set; } = null!;
        // Ventas
        public DbSet<Venta> Ventas { get; set; } = null!;
        public DbSet<VentaDetalle> VentaDetalles { get; set; } = null!;
        // Facturación Electrónica / Comprobantes
        public DbSet<SerieFiscal> SeriesFiscales { get; set; } = null!;
        public DbSet<Comprobante> Comprobantes { get; set; } = null!;
        public DbSet<ComprobanteAuditoria> ComprobantesAuditoria { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<ClienteContacto> ClienteContactos { get; set; } = null!;
        public DbSet<ClienteDireccion> ClienteDirecciones { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; } = null!;
        public DbSet<Almacen> Almacenes { get; set; } = null!;
        public DbSet<Ubicacion> Ubicaciones { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; } = null!;
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; } = null!;
        public DbSet<Transferencia> Transferencias { get; set; } = null!;
        public DbSet<DetalleTransferencia> DetallesTransferencia { get; set; } = null!;
        public DbSet<InventarioFisico> InventariosFisicos { get; set; } = null!;
        public DbSet<DetalleInventarioFisico> DetallesInventarioFisico { get; set; } = null!;

        // Entidades de Seguridad (Fase 1.4.4.3)
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<UsuarioRol> UsuariosRoles { get; set; } = null!;
        public DbSet<Permiso> Permisos { get; set; } = null!;
        public DbSet<RolPermiso> RolesPermisos { get; set; } = null!;
        public DbSet<ConfiguracionEmpresa> ConfiguracionesEmpresa { get; set; } = null!;
        public DbSet<PreferenciaUsuario> PreferenciasUsuario { get; set; } = null!;
        public DbSet<UsuarioSucursal> UsuariosSucursales { get; set; } = null!;

        // Finanzas
        public DbSet<MedioPago> MediosPago { get; set; } = null!;
        public DbSet<CanalPago> CanalesPago { get; set; } = null!;
        public DbSet<ProveedorPago> ProveedoresPago { get; set; } = null!;
        public DbSet<CuentaTesoreria> CuentasTesoreria { get; set; } = null!;
        public DbSet<Caja> Cajas { get; set; } = null!;
        public DbSet<AperturaCaja> AperturasCajas { get; set; } = null!;
        public DbSet<MovimientoCaja> MovimientosCajas { get; set; } = null!;
        public DbSet<MovimientoTesoreria> MovimientosTesoreria { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<AplicacionPago> AplicacionesPago { get; set; } = null!;
        public DbSet<ObligacionCxC> ObligacionesCxC { get; set; } = null!;
        public DbSet<MovimientoCxC> MovimientosCxC { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Compras y Proveedores Configure Delete Behavior
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Empresa)
                .WithMany()
                .HasForeignKey(c => c.IdEmpresa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Sucursal)
                .WithMany()
                .HasForeignKey(c => c.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany()
                .HasForeignKey(c => c.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuarioLog)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompraDetalle>()
                .HasOne(cd => cd.Producto)
                .WithMany()
                .HasForeignKey(cd => cd.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // Centralización de auditoría automática (FASE 1.4.4.3 - Etapa 2)
        // Por qué se centralizó: Evita que cada repositorio o controlador deba inyectar 
        // ICurrentUserService y escribir las fechas y usuarios. Disminuye la repetición
        // de código y garantiza que ninguna entidad auditable quede sin rastrear.
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var currentUser = _currentUserService.Username;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    var fechaCreacionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "FechaCreacion");
                    // En SQL Server, SYSDATETIME() es el default. Asignamos Now si la entidad aún no tiene fecha.
                    if (fechaCreacionProp != null && fechaCreacionProp.Metadata.ClrType == typeof(DateTime) && (DateTime)fechaCreacionProp.CurrentValue! == default)
                    {
                        fechaCreacionProp.CurrentValue = DateTime.Now;
                    }
                    else if (fechaCreacionProp != null && fechaCreacionProp.Metadata.ClrType == typeof(DateTime?) && fechaCreacionProp.CurrentValue == null)
                    {
                        fechaCreacionProp.CurrentValue = DateTime.Now;
                    }

                    var usuarioCreacionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UsuarioCreacion");
                    if (usuarioCreacionProp != null && usuarioCreacionProp.Metadata.ClrType == typeof(string))
                    {
                        // Respetamos el bootstrap: si no hay usuario autenticado, permitimos nulo (ya que en SQL es NULLable).
                        if (currentUser != null && string.IsNullOrEmpty((string?)usuarioCreacionProp.CurrentValue))
                        {
                            usuarioCreacionProp.CurrentValue = currentUser;
                        }
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Prevenir sobrescritura de campos de creación si el cliente intentó alterarlos
                    var fechaCreacionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "FechaCreacion");
                    if (fechaCreacionProp != null)
                    {
                        fechaCreacionProp.IsModified = false;
                    }

                    var usuarioCreacionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UsuarioCreacion");
                    if (usuarioCreacionProp != null)
                    {
                        usuarioCreacionProp.IsModified = false;
                    }

                    var fechaModProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "FechaModificacion");
                    if (fechaModProp != null && (fechaModProp.Metadata.ClrType == typeof(DateTime) || fechaModProp.Metadata.ClrType == typeof(DateTime?)))
                    {
                        fechaModProp.CurrentValue = DateTime.Now;
                    }

                    var usuarioModProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UsuarioModificacion");
                    if (usuarioModProp != null && usuarioModProp.Metadata.ClrType == typeof(string))
                    {
                        if (currentUser != null)
                        {
                            usuarioModProp.CurrentValue = currentUser;
                        }
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
