using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Datos.Repositorios;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SistemaVentasMotorrepuestos.API.Configuracion;
using SistemaVentasMotorrepuestos.API.Servicios.Autorizacion;
using SistemaVentasMotorrepuestos.API.Middleware;
using SistemaVentasMotorrepuestos.API.Servicios.Workers;
using SistemaVentasMotorrepuestos.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Railway publica el puerto de cada servicio mediante la variable PORT.
// Kestrel debe escuchar en todas las interfaces dentro del contenedor para
// que el proxy de Railway pueda alcanzar la API.
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort) && int.TryParse(railwayPort, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// En desarrollo no depender del proveedor Windows Event Log, que requiere
// permisos administrativos y puede convertir un error registrable en un fallo
// de la solicitud. Producción conserva los proveedores predeterminados.
if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

// ========================================================
// 1. CONFIGURACIÃ“N DE SERVICIOS (Dependency Injection)
// ========================================================

// Controladores
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ConfiguraciÃ³n de OpenAPI / Swagger (Preparado para el futuro)
builder.Services.AddOpenApi();

// Health Checks (Fase 1.4.4.8)
// Incluye chequeo genÃ©rico y comprobaciÃ³n robusta nativa de SQL Server sin exponer connection strings
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("SQLServer", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" });

// Rate Limiting (Fase 1.4.4.8)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    var loginLimit = builder.Configuration.GetValue<int>("RateLimiting:LoginLimit", 5);
    var globalLimit = builder.Configuration.GetValue<int>("RateLimiting:GlobalLimit", 100);

    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = loginLimit;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("GlobalPolicy", opt =>
    {
        opt.PermitLimit = globalLimit;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// ConfiguraciÃ³n de CORS dinÃ¡mica segÃºn el entorno (Fase 1.4.4.8)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();
        
        if (builder.Environment.IsDevelopment())
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins);
            }
            else 
            {
                // Fallback solo en desarrollo si no hay configuracion
                policy.AllowAnyOrigin();
            }
        }
        else
        {
            // PROD: Nunca AllowAnyOrigin. Siempre restringido por configuracion (appsettings.json)
            var prodOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            if (prodOrigins != null && prodOrigins.Length > 0)
            {
                policy.WithOrigins(prodOrigins);
            }
        }
    });
});

// ConfiguraciÃ³n SQL Server (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.CommandTimeout(60));
});

// InyecciÃ³n de Repositorios GenÃ©ricos y Unidad de Trabajo (Lifetime Scoped igual que DbContext)
builder.Services.AddScoped(typeof(IRepositorioBase<>), typeof(RepositorioBase<>));
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

// Infraestructura de AuditorÃ­a y Seguridad
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Infraestructura de AutorizaciÃ³n (Fase 1.4.4.7)
builder.Services.AddScoped<IPermisoAuthorizationService, PermisoAuthorizationService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
// Habilitar AutorizaciÃ³n en los servicios (necesario para el uso de [Authorize])
builder.Services.AddAuthorization();

// ConfiguraciÃ³n Identity / JWT
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart(); // Obliga a validar la configuraciÃ³n al arrancar. La API falla si el secreto es dÃ©bil o no existe.

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        
        // Evitamos instanciar SymmetricSecurityKey con arreglos vacÃ­os para prevenir excepciones 
        // no controladas durante la fase de inyecciÃ³n de dependencias. 
        // El validador (ValidateOnStart) detendrÃ¡ el arranque limpiamente si la Key es invÃ¡lida.
        SecurityKey? signingKey = null;
        if (!string.IsNullOrEmpty(jwtOptions.Key) && jwtOptions.Key.Length >= 32)
        {
            signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Tolerancia cero por defecto (alta seguridad en expiraciÃ³n).
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// InyecciÃ³n de Servicios de Negocio
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.INumeradorPedido, SistemaVentasMotorrepuestos.API.Servicios.NumeradorPedido>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IPedidoRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.PedidoRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IVentaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.VentaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IPedidoService, SistemaVentasMotorrepuestos.API.Servicios.PedidoService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.INumeradorVenta, SistemaVentasMotorrepuestos.API.Servicios.NumeradorVenta>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IVentaService, SistemaVentasMotorrepuestos.API.Servicios.VentaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IInventarioService, SistemaVentasMotorrepuestos.API.Servicios.InventarioService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IClienteService, SistemaVentasMotorrepuestos.API.Servicios.ClienteService>();

// Tax Engine DI
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.ITaxConfigurationResolver, SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.TaxConfigurationResolver>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.ITaxRuleResolver, SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.TaxRuleResolver>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.ITaxRoundingService, SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.TaxRoundingService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.ITaxCalculator, SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.TaxCalculator>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.ITaxEngine, SistemaVentasMotorrepuestos.API.Servicios.TaxEngine.TaxEngine>();

// Repositorios Financieros
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IObligacionCxCRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.ObligacionCxCRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IMovimientoCxCRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.MovimientoCxCRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IPagoRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.PagoRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IAplicacionPagoRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.AplicacionPagoRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.ICajaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.CajaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IAperturaCajaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.AperturaCajaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IMovimientoCajaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.MovimientoCajaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.ITesoreriaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.TesoreriaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IMovimientoTesoreriaRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.MovimientoTesoreriaRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.ISerieFiscalRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.SerieFiscalRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IComprobanteRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.ComprobanteRepository>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Datos.Interfaces.IProveedorPagoRepository, SistemaVentasMotorrepuestos.API.Datos.Repositorios.ProveedorPagoRepository>();

// Servicios Financieros
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IObligacionCxCService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.ObligacionCxCService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IMovimientoCxCService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.MovimientoCxCService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IPagoService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.PagoService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IAplicacionPagoService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.AplicacionPagoService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.ICajaService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.CajaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IAperturaCajaService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.AperturaCajaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IMovimientoCajaService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.MovimientoCajaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.ITesoreriaService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.TesoreriaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IMovimientoTesoreriaService, SistemaVentasMotorrepuestos.API.Servicios.Financieros.MovimientoTesoreriaService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras.IProveedoresService, SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Compras.ProveedoresService>();
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Compras.IComprasService, SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Compras.ComprasService>();

// â”€â”€ FacturaciÃ³n ElectrÃ³nica â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// ComprobanteService: lÃ³gica de negocio (Scoped, igual que DbContext)
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IComprobanteService,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.ComprobanteService>();

// Construye el DTO fiscal consolidado consumido por los workers de emisiÃ³n y consulta.
builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IDocumentBuilder,
    SistemaVentasMotorrepuestos.API.Servicios.DocumentBuilder>();

// Proveedor temporal de desarrollo/pruebas: NO representa SUNAT, OSE ni PSE,
// no realiza HTTP externo y no debe utilizarse como proveedor productivo.
builder.Services.AddSingleton<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IProveedorFacturacionElectronica,
    SistemaVentasMotorrepuestos.API.Servicios.Fakes.ProveedorFacturacionElectronicaFake>();

// Delivery 5.5 aislado: composiciÃ³n exclusivamente Fake, sin HTTP, SMTP ni WhatsApp real.
builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion.IEmailDeliveryAdapter,
    SistemaVentasMotorrepuestos.API.Servicios.Fakes.EmailDeliveryAdapterFake>();
builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion.IWhatsAppDeliveryAdapter,
    SistemaVentasMotorrepuestos.API.Servicios.Fakes.WhatsAppDeliveryAdapterFake>();
builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IComprobanteDeliveryService,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.ComprobanteDeliveryService>();

// Lease/Fencing Manager (Scoped para que pueda inyectar ApplicationDbContext)
builder.Services.AddScoped<LeaseFencingManager>();

// â”€â”€ XML / UBL SerializaciÃ³n (Etapa 5.2) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
// Singleton: stateless, no accede a BD. Cachea XmlSerializer internamente.
builder.Services.AddSingleton<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IXmlSchemaValidator,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.UblXsdSchemaValidator>();

builder.Services.AddSingleton<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IXmlSerializerService,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.XmlSerializerService>();
builder.Services.AddSingleton<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.ISunatFiscalValidator,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.SunatFiscalValidator>();

// Storage local controlado para artefactos XML de pre-emisiÃ³n 5.6.
// Scoped evita introducir estado global y es compatible con el scope de cada lote del worker.
builder.Services.AddScoped<SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IFileStorageService>(serviceProvider =>
{
    var configuredPath = builder.Configuration["Facturacion:Storage:BasePath"];
    if (string.IsNullOrWhiteSpace(configuredPath))
        throw new InvalidOperationException("La configuraciÃ³n Facturacion:Storage:BasePath es obligatoria.");

    var basePath = Path.IsPathRooted(configuredPath)
        ? configuredPath
        : Path.Combine(builder.Environment.ContentRootPath, configuredPath);

    return new SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.LocalFileStorageService(
        basePath,
        serviceProvider.GetRequiredService<ILogger<SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.LocalFileStorageService>>());
});

builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IComprobanteArtifactService,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.ComprobanteArtifactService>();

// Documento imprimible (vista previa/PDF/ticket) del cierre de venta.
builder.Services.AddSingleton<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion.IPdfGenerator,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.Pdf.PdfSharpGenerator>();
builder.Services.AddScoped<
    SistemaVentasMotorrepuestos.API.Servicios.Interfaces.IDocumentoImpresionService,
    SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion.DocumentoImpresionService>();

// Workers de facturaciÃ³n: su registro se controla mediante Facturacion:Workers:Enabled.
// Si la configuraciÃ³n falta, el comportamiento fail-safe es no registrarlos.
builder.Services.AddFacturacionHostedWorkers(builder.Configuration);

var app = builder.Build();

// Aplica migraciones pendientes solo cuando se habilita explícitamente.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup", false))
{
    await ApplyPendingMigrationsAsync(app.Services);
}

// Bootstrap RBAC opt-in. No se ejecuta por defecto y nunca crea usuarios ni contraseñas.
// Para aplicarlo manualmente debe habilitarse fuera del sandbox con:
// SecurityBootstrap__Enabled=true
if (builder.Configuration.GetValue<bool>("SecurityBootstrap:Enabled", false))
{
    await BootstrapRbacAsync(app.Services);
}
if (builder.Configuration.GetValue<bool>("SecurityBootstrap:Users:Enabled", false))
{
    await BootstrapOperationalUsersAsync(app.Services);
}
if (builder.Configuration.GetValue<bool>("SecurityBootstrap:ExitAfter", false))
{
    return;
}

// ========================================================
// 2. CONFIGURACIÃ“N DEL PIPELINE HTTP (Middleware)
// ========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // En el futuro se puede integrar Swagger UI aquÃ­
}

// Manejo Global de Excepciones (Fase 1.4.4.8)
app.UseMiddleware<ManejoExcepcionesMiddleware>();

// Headers de seguridad estrictos (Fase 1.4.4.8)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Railway termina TLS en su borde y reenvía HTTP al contenedor. La propia
// plataforma ya fuerza HTTPS públicamente; redirigir otra vez dentro de la
// aplicación produciría un bucle. En los demás entornos se conserva HTTPS.
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")))
{
    app.UseHttpsRedirection();
}

// Habilitar CORS
app.UseCors("CorsPolicy");

// Rate Limiter
app.UseRateLimiter();

app.UseAuthentication(); // Primero autentica (Â¿quiÃ©n eres?)
app.UseAuthorization();  // Luego autoriza (Â¿quÃ© puedes hacer?)

// Endpoints de Health (Fase 1.4.4.8)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // liveness check bÃ¡sico (no prueba SQL)
}).RequireRateLimiting("GlobalPolicy");

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") // readiness check (prueba SQL Server)
}).RequireRateLimiting("GlobalPolicy");

// Mapeo de Controladores con lÃ­mite global de seguridad (Fase 1.4.4.8)
app.MapControllers().RequireRateLimiting("GlobalPolicy");
app.MapHub<InventarioHub>("/hubs/inventario");
app.MapHub<ComprasHub>("/hubs/compras");

app.Run();

static async Task BootstrapRbacAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var now = DateTime.UtcNow;
    const string actor = "security-bootstrap";

    var catalog = new Dictionary<string, (string Name, string Description)>(StringComparer.OrdinalIgnoreCase)
    {
        ["PRODUCTOS_CONSULTAR"] = ("Consultar productos", "Consultar productos, precios y existencias necesarias para operar."),
        ["REPORTES_CONSULTAR"] = ("Consultar reportes", "Consultar reportes operativos autorizados."),
        ["CONFIGURACION_GESTIONAR"] = ("Gestionar configuracion", "Administrar la configuracion del sistema."),
        ["CLIENTES_CONSULTAR"] = ("Consultar clientes", "Consultar y registrar clientes mediante los endpoints autorizados."),
        ["INVENTARIO_CONSULTAR"] = ("Consultar inventario", "Consultar existencias, almacenes y resumen de inventario."),
        ["INVENTARIO_MODIFICAR"] = ("Modificar inventario", "Registrar ajustes de inventario."),
        ["PEDIDOS_CREAR"] = ("Crear pedidos", "Crear pedidos."),
        ["PEDIDOS_GESTIONAR"] = ("Gestionar pedidos", "Consultar y cambiar el estado de pedidos."),
        ["PRODUCTOS_GESTIONAR"] = ("Gestionar productos", "Consultar y administrar productos y catálogos."),
        ["VENTAS_CREAR"] = ("Crear ventas", "Crear ventas y registrar sus pagos."),
        ["VENTAS_GESTIONAR"] = ("Gestionar ventas", "Consultar y gestionar ventas, caja y comprobantes."),
        ["VENTAS_ANULAR"] = ("Anular ventas", "Anular ventas."),
        ["COMPRAS_GESTIONAR"] = ("Gestionar compras", "Consultar, registrar y cambiar el estado de compras."),
        ["PROVEEDORES_CONSULTAR"] = ("Consultar proveedores", "Consultar proveedores activos."),
    };

    await using var transaction = await db.Database.BeginTransactionAsync();

    foreach (var item in catalog)
    {
        var permiso = await db.Permisos.SingleOrDefaultAsync(p => p.CodigoSistema == item.Key);
        if (permiso is null)
        {
            db.Permisos.Add(new Permiso
            {
                CodigoSistema = item.Key,
                Nombre = item.Value.Name,
                Descripcion = item.Value.Description,
                Activo = true,
                FechaCreacion = now,
                UsuarioCreacion = actor
            });
        }
        else if (!permiso.Activo)
        {
            permiso.Activo = true;
            permiso.FechaModificacion = now;
            permiso.UsuarioModificacion = actor;
        }
    }

    await db.SaveChangesAsync();

    var admin = await db.Roles.SingleOrDefaultAsync(r => r.NombreRol == "Admin");
    if (admin is null)
    {
        admin = new Rol
        {
            NombreRol = "Admin",
            Descripcion = "Administración total del sistema.",
            Activo = true,
            FechaCreacion = now,
            UsuarioCreacion = actor
        };
        db.Roles.Add(admin);
        await db.SaveChangesAsync();
    }
    else if (!admin.Activo)
    {
        admin.Activo = true;
        admin.FechaModificacion = now;
        admin.UsuarioModificacion = actor;
        await db.SaveChangesAsync();
    }

    var permisos = await db.Permisos
        .Where(p => catalog.Keys.Contains(p.CodigoSistema) && p.Activo)
        .ToListAsync();

    var roles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = catalog.Keys.ToArray(),
        ["Vendedor"] = ["VENTAS_CREAR", "CLIENTES_CONSULTAR", "PEDIDOS_CREAR", "INVENTARIO_CONSULTAR", "PRODUCTOS_CONSULTAR"],
        ["Cajero"] = ["VENTAS_CREAR", "VENTAS_GESTIONAR", "PRODUCTOS_CONSULTAR"],
        ["Almacén"] = ["INVENTARIO_CONSULTAR", "INVENTARIO_MODIFICAR", "PRODUCTOS_GESTIONAR", "COMPRAS_GESTIONAR", "PROVEEDORES_CONSULTAR"]
    };

    foreach (var definition in roles)
    {
        var role = definition.Key.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            ? admin
            : await db.Roles.SingleOrDefaultAsync(r => r.NombreRol == definition.Key);

        if (role is null)
        {
            role = new Rol
            {
                NombreRol = definition.Key,
                Descripcion = $"Rol operativo {definition.Key}.",
                Activo = true,
                FechaCreacion = now,
                UsuarioCreacion = actor
            };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }

        var rolePermissionIds = await db.RolesPermisos
            .Where(rp => rp.IdRol == role.IdRol)
            .Select(rp => rp.IdPermiso)
            .ToHashSetAsync();

        var allowedCodes = definition.Value.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var staleAssignments = await db.RolesPermisos
            .Include(rp => rp.Permiso)
            .Where(rp => rp.IdRol == role.IdRol && !allowedCodes.Contains(rp.Permiso.CodigoSistema))
            .ToListAsync();
        if (staleAssignments.Count > 0)
            db.RolesPermisos.RemoveRange(staleAssignments);

        foreach (var permiso in permisos.Where(p => definition.Value.Contains(p.CodigoSistema, StringComparer.OrdinalIgnoreCase)))
        {
            if (!rolePermissionIds.Contains(permiso.IdPermiso))
            {
                db.RolesPermisos.Add(new RolPermiso { IdRol = role.IdRol, IdPermiso = permiso.IdPermiso });
            }
        }
    }

    // La matriz aprobada exige que Almacén tenga explícitamente lectura y gestión de productos.
    var almacenRole = await db.Roles.SingleAsync(r => r.NombreRol == "Almac\u00e9n");
    var productosConsultar = await db.Permisos.SingleAsync(p => p.CodigoSistema == "PRODUCTOS_CONSULTAR");
    var almacenReadAssignment = await db.RolesPermisos.FindAsync(almacenRole.IdRol, productosConsultar.IdPermiso);
    if (almacenReadAssignment is null)
        db.RolesPermisos.Add(new RolPermiso { IdRol = almacenRole.IdRol, IdPermiso = productosConsultar.IdPermiso });

    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}

static async Task BootstrapOperationalUsersAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var now = DateTime.UtcNow;
    const string actor = "security-bootstrap-users";

    var definitions = new[]
    {
        (Username: "vendedor_user", Email: "vendedor_user@local", Role: "Vendedor", PasswordVariable: "SecurityBootstrap__Users__VendedorPassword"),
        (Username: "cajero_user", Email: "cajero_user@local", Role: "Cajero", PasswordVariable: "SecurityBootstrap__Users__CajeroPassword"),
        (Username: "almacen_user", Email: "almacen_user@local", Role: "Almacén", PasswordVariable: "SecurityBootstrap__Users__AlmacenPassword")
    };

    await using var transaction = await db.Database.BeginTransactionAsync();
    foreach (var definition in definitions)
    {
        var existing = await db.Usuarios.SingleOrDefaultAsync(u => u.Username == definition.Username);
        var role = await db.Roles.SingleOrDefaultAsync(r => r.NombreRol == definition.Role && r.Activo);
        if (role is null)
            throw new InvalidOperationException($"No existe el rol activo requerido: {definition.Role}. Ejecute primero SecurityBootstrap:Enabled=true.");

        if (existing is null)
        {
            var password = Environment.GetEnvironmentVariable(definition.PasswordVariable);
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Falta la variable de entorno {definition.PasswordVariable} para crear {definition.Username}.");

            existing = new Usuario
            {
                Username = definition.Username,
                Email = definition.Email,
                PasswordHash = hasher.HashPassword(password),
                Activo = true,
                FechaCreacion = now,
                UsuarioCreacion = actor
            };
            db.Usuarios.Add(existing);
            await db.SaveChangesAsync();
        }

        if (!existing.Activo)
        {
            existing.Activo = true;
            existing.FechaModificacion = now;
            existing.UsuarioModificacion = actor;
        }

        if (!await db.UsuariosRoles.AnyAsync(ur => ur.IdUsuario == existing.IdUsuario && ur.IdRol == role.IdRol))
            db.UsuariosRoles.Add(new UsuarioRol { IdUsuario = existing.IdUsuario, IdRol = role.IdRol });
    }

    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}

static async Task ApplyPendingMigrationsAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");

    var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
    if (pending.Count == 0)
    {
        logger.LogInformation("La base de datos ya está actualizada; no hay migraciones pendientes.");
        return;
    }

    logger.LogInformation("Se aplicarán {Count} migraciones pendientes de EF Core.", pending.Count);
    await db.Database.MigrateAsync();
    logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");
}

public partial class Program { }

