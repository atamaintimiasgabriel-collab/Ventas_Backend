using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;
using SistemaVentasMotorrepuestos.API.DTOs.Configuracion;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Autorizacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.DTOs.TaxEngine;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1;

[ApiController]
[Route("api/v1/configuracion")]
[RequireTenantPermission("CONFIGURACION_GESTIONAR")]
public sealed class ConfiguracionController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasherService _passwordHasher;
    public ConfiguracionController(ApplicationDbContext db, IPasswordHasherService passwordHasher) { _db = db; _passwordHasher = passwordHasher; }

    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId();
        var userId = UsuarioId();
        var empresa = await _db.Empresas.AsNoTracking().SingleOrDefaultAsync(x => x.IdEmpresa == empresaId && x.Activo, cancellationToken);
        if (empresa is null) return NotFound(new { mensaje = "Empresa activa no encontrada." });
        var sistema = await _db.ConfiguracionesEmpresa.AsNoTracking().SingleOrDefaultAsync(x => x.IdEmpresa == empresaId, cancellationToken);
        var preferencias = await _db.PreferenciasUsuario.AsNoTracking().SingleOrDefaultAsync(x => x.IdUsuario == userId, cancellationToken);
        var sucursales = await _db.Sucursales.AsNoTracking().Where(x => x.IdEmpresa == empresaId).OrderBy(x => x.Nombre)
            .Select(x => new { x.IdSucursal, x.Nombre, x.CodigoEstablecimiento, x.Direccion, x.Telefono, x.Activo }).ToListAsync(cancellationToken);
        var usuarios = await _db.Usuarios.AsNoTracking().OrderBy(x => x.Username).Select(x => new { x.IdUsuario, x.Username, x.Email, x.Activo, Roles = x.UsuariosRoles.Where(ur => ur.Rol.Activo).Select(ur => ur.Rol.NombreRol).ToList(), RolesIds = x.UsuariosRoles.Select(ur => ur.IdRol).ToList(), SucursalesIds = _db.UsuariosSucursales.Where(us => us.IdUsuario == x.IdUsuario).Select(us => us.IdSucursal).ToList(), IdSucursalPrincipal = _db.UsuariosSucursales.Where(us => us.IdUsuario == x.IdUsuario && us.EsPrincipal).Select(us => (int?)us.IdSucursal).FirstOrDefault() }).ToListAsync(cancellationToken);
        var roles = await _db.Roles.AsNoTracking().OrderBy(x => x.NombreRol).Select(x => new { x.IdRol, x.NombreRol, x.Descripcion, x.Activo, Usuarios = x.UsuariosRoles.Count, PermisosIds = x.RolesPermisos.Select(rp => rp.IdPermiso).ToList() }).ToListAsync(cancellationToken);
        var permisos = await _db.Permisos.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.CodigoSistema).Select(x => new { x.IdPermiso, x.CodigoSistema, x.Nombre, x.Descripcion }).ToListAsync(cancellationToken);
        var series = await _db.SeriesFiscales.AsNoTracking().Where(x => x.IdEmpresa == empresaId).OrderBy(x => x.TipoComprobante).ThenBy(x => x.Serie)
            .Select(x => new { x.IdSerieFiscal, x.TipoComprobante, x.Serie, x.CorrelativoActual, x.IdSucursal, x.Activo }).ToListAsync(cancellationToken);
        EmpresaTaxConfigDTO? impuestos = null;
        try { impuestos = string.IsNullOrWhiteSpace(empresa.ConfiguracionFiscalJson) ? null : JsonSerializer.Deserialize<EmpresaTaxConfigDTO>(empresa.ConfiguracionFiscalJson); } catch (JsonException) { }
        return Ok(new { empresa, sistema, preferencias, sucursales, usuarios, roles, permisos, impuestos, series, indicadores = new { sucursalesActivas = sucursales.Count(x => x.Activo), usuarios = usuarios.Count(x => x.Activo), roles = roles.Count(x => x.Activo), comprobantes = series.Select(x => x.TipoComprobante).Distinct().Count() } });
    }

    [HttpPut("sistema")]
    public async Task<IActionResult> GuardarSistema([FromBody] ActualizarSistemaRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId();
        var item = await _db.ConfiguracionesEmpresa.SingleOrDefaultAsync(x => x.IdEmpresa == empresaId, cancellationToken);
        if (item is null) { item = new ConfiguracionEmpresa { IdEmpresa = empresaId }; _db.ConfiguracionesEmpresa.Add(item); }
        item.Moneda = request.Moneda; item.Idioma = request.Idioma; item.ZonaHoraria = request.ZonaHoraria; item.FormatoFecha = request.FormatoFecha; item.FormatoHora = request.FormatoHora;
        item.ControlStockNegativo = request.ControlStockNegativo; item.AlertasStock = request.AlertasStock; item.RequiereCajaAbierta = request.RequiereCajaAbierta;
        await _db.SaveChangesAsync(cancellationToken); return Ok(item);
    }

    [HttpPut("empresa")]
    public async Task<IActionResult> GuardarEmpresa([FromBody] ActualizarEmpresaRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId();
        var empresa = await _db.Empresas.SingleOrDefaultAsync(x => x.IdEmpresa == empresaId && x.Activo, cancellationToken);
        if (empresa is null) return NotFound(new { mensaje = "Empresa activa no encontrada." });
        if (await _db.Empresas.AnyAsync(x => x.IdEmpresa != empresaId && x.RucDocumento == request.RucDocumento, cancellationToken)) return Conflict(new { mensaje = "El RUC ya pertenece a otra empresa." });
        empresa.RazonSocial = request.RazonSocial.Trim(); empresa.RucDocumento = request.RucDocumento; empresa.DireccionFiscal = request.DireccionFiscal?.Trim(); empresa.Telefono = request.Telefono?.Trim(); empresa.EmailContacto = request.EmailContacto?.Trim(); empresa.NombreComercial = request.NombreComercial?.Trim(); empresa.SitioWeb = request.SitioWeb?.Trim();
        await _db.SaveChangesAsync(cancellationToken); return Ok(empresa);
    }

    [HttpPut("mis-preferencias")]
    public async Task<IActionResult> GuardarPreferencias([FromBody] ActualizarPreferenciaUsuarioRequest request, CancellationToken cancellationToken)
    {
        var userId = UsuarioId();
        var item = await _db.PreferenciasUsuario.SingleOrDefaultAsync(x => x.IdUsuario == userId, cancellationToken);
        if (item is null) { item = new PreferenciaUsuario { IdUsuario = userId }; _db.PreferenciasUsuario.Add(item); }
        item.Autoguardado = request.Autoguardado; item.Notificaciones = request.Notificaciones; item.ReproducirSonidos = request.ReproducirSonidos;
        item.Tema = request.Tema; item.ModoOscuro = request.ModoOscuro; item.Animaciones = request.Animaciones; item.MostrarImagenesCatalogo = request.MostrarImagenesCatalogo;
        item.TamanoFuente = request.TamanoFuente; item.DensidadInformacion = request.DensidadInformacion;
        await _db.SaveChangesAsync(cancellationToken); return Ok(item);
    }

    [HttpPut("impuestos")]
    public async Task<IActionResult> GuardarImpuestos([FromBody] ActualizarImpuestosRequest request, CancellationToken cancellationToken)
    {
        var empresa = await _db.Empresas.SingleOrDefaultAsync(x => x.IdEmpresa == EmpresaId() && x.Activo, cancellationToken);
        if (empresa is null) return NotFound(new { mensaje = "Empresa activa no encontrada." });
        EmpresaTaxConfigDTO config;
        try { config = string.IsNullOrWhiteSpace(empresa.ConfiguracionFiscalJson) ? new EmpresaTaxConfigDTO() : JsonSerializer.Deserialize<EmpresaTaxConfigDTO>(empresa.ConfiguracionFiscalJson) ?? new EmpresaTaxConfigDTO(); } catch (JsonException) { return Conflict(new { mensaje = "La configuración fiscal almacenada no tiene un formato válido." }); }
        config.IdEmpresa = empresa.IdEmpresa; config.PreciosIncluyenImpuestos = request.PreciosIncluyenImpuestos;
        var regla = config.ReglasTributarias.FirstOrDefault(x => x.ClasificacionTributaria == "GRA" && x.FechaFin is null);
        if (regla is null) config.ReglasTributarias.Add(new TaxRuleDTO { ClasificacionTributaria = "GRA", ConceptoTributario = "IGV", TasaTributaria = request.TasaIgv, FechaInicio = DateTime.UtcNow.Date });
        else regla.TasaTributaria = request.TasaIgv;
        empresa.ConfiguracionFiscalJson = JsonSerializer.Serialize(config); empresa.FechaModificacion = DateTime.UtcNow; empresa.UsuarioModificacion = User.Identity?.Name;
        await _db.SaveChangesAsync(cancellationToken); return Ok(config);
    }

    [HttpPost("sucursales")]
    public async Task<IActionResult> CrearSucursal([FromBody] GuardarSucursalRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId();
        var nombre = request.Nombre.Trim();
        if (await _db.Sucursales.AnyAsync(x => x.IdEmpresa == empresaId && x.Nombre == nombre, cancellationToken)) return Conflict(new { mensaje = "Ya existe una sucursal con ese nombre." });
        var sucursal = new Sucursal { IdEmpresa = empresaId, Nombre = nombre, CodigoEstablecimiento = request.CodigoEstablecimiento?.Trim(), Direccion = request.Direccion?.Trim(), Telefono = request.Telefono?.Trim(), Activo = true };
        _db.Sucursales.Add(sucursal); await _db.SaveChangesAsync(cancellationToken); return CreatedAtAction(nameof(Obtener), new { idSucursal = sucursal.IdSucursal }, sucursal);
    }

    [HttpPut("sucursales/{idSucursal:int}")]
    public async Task<IActionResult> ActualizarSucursal(int idSucursal, [FromBody] GuardarSucursalRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId(); var nombre = request.Nombre.Trim();
        var sucursal = await _db.Sucursales.SingleOrDefaultAsync(x => x.IdSucursal == idSucursal && x.IdEmpresa == empresaId, cancellationToken);
        if (sucursal is null) return NotFound(new { mensaje = "Sucursal no encontrada." });
        if (await _db.Sucursales.AnyAsync(x => x.IdEmpresa == empresaId && x.IdSucursal != idSucursal && x.Nombre == nombre, cancellationToken)) return Conflict(new { mensaje = "Ya existe una sucursal con ese nombre." });
        sucursal.Nombre = nombre; sucursal.CodigoEstablecimiento = request.CodigoEstablecimiento?.Trim(); sucursal.Direccion = request.Direccion?.Trim(); sucursal.Telefono = request.Telefono?.Trim();
        await _db.SaveChangesAsync(cancellationToken); return Ok(sucursal);
    }

    [HttpPatch("sucursales/{idSucursal:int}/estado")]
    public async Task<IActionResult> CambiarEstadoSucursal(int idSucursal, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId();
        var sucursal = await _db.Sucursales.SingleOrDefaultAsync(x => x.IdSucursal == idSucursal && x.IdEmpresa == empresaId, cancellationToken);
        if (sucursal is null) return NotFound(new { mensaje = "Sucursal no encontrada." });
        if (!activo && !await _db.Sucursales.AnyAsync(x => x.IdEmpresa == empresaId && x.IdSucursal != idSucursal && x.Activo, cancellationToken)) return Conflict(new { mensaje = "Debe permanecer al menos una sucursal activa." });
        sucursal.Activo = activo; await _db.SaveChangesAsync(cancellationToken); return Ok(sucursal);
    }

    [HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuario([FromBody] GuardarUsuarioConfiguracionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { mensaje = "La contraseña inicial es obligatoria." });
        var username = request.Username.Trim(); var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(x => x.Username == username || x.Email == email, cancellationToken)) return Conflict(new { mensaje = "El usuario o correo ya existe." });
        var roles = await _db.Roles.Where(x => x.Activo && request.RolesIds.Distinct().Contains(x.IdRol)).ToListAsync(cancellationToken);
        if (roles.Count != request.RolesIds.Distinct().Count()) return BadRequest(new { mensaje = "Uno o más roles no son válidos." });
        var usuario = new Usuario { Username = username, Email = email, PasswordHash = _passwordHasher.HashPassword(request.Password), Activo = true, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = User.Identity?.Name };
        var sucursales = await _db.Sucursales.Where(x => x.IdEmpresa == EmpresaId() && x.Activo && request.SucursalesIds.Distinct().Contains(x.IdSucursal)).ToListAsync(cancellationToken);
        if (sucursales.Count != request.SucursalesIds.Distinct().Count()) return BadRequest(new { mensaje = "Una o más sucursales no son válidas." });
        if (request.IdSucursalPrincipal.HasValue && !request.SucursalesIds.Contains(request.IdSucursalPrincipal.Value)) return BadRequest(new { mensaje = "La sucursal principal debe estar asignada." });
        foreach (var rol in roles) usuario.UsuariosRoles.Add(new UsuarioRol { IdRol = rol.IdRol });
        _db.Usuarios.Add(usuario); await _db.SaveChangesAsync(cancellationToken);
        foreach (var sucursal in sucursales) _db.UsuariosSucursales.Add(new UsuarioSucursal { IdUsuario = usuario.IdUsuario, IdSucursal = sucursal.IdSucursal, EsPrincipal = request.IdSucursalPrincipal == sucursal.IdSucursal });
        await _db.SaveChangesAsync(cancellationToken); return CreatedAtAction(nameof(Obtener), new { idUsuario = usuario.IdUsuario }, new { usuario.IdUsuario, usuario.Username, usuario.Email });
    }

    [HttpPut("usuarios/{idUsuario:int}")]
    public async Task<IActionResult> ActualizarUsuario(int idUsuario, [FromBody] GuardarUsuarioConfiguracionRequest request, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.Include(x => x.UsuariosRoles).SingleOrDefaultAsync(x => x.IdUsuario == idUsuario, cancellationToken);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });
        var username = request.Username.Trim(); var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(x => x.IdUsuario != idUsuario && (x.Username == username || x.Email == email), cancellationToken)) return Conflict(new { mensaje = "El usuario o correo ya existe." });
        var roles = await _db.Roles.Where(x => x.Activo && request.RolesIds.Distinct().Contains(x.IdRol)).ToListAsync(cancellationToken);
        if (roles.Count != request.RolesIds.Distinct().Count()) return BadRequest(new { mensaje = "Uno o más roles no son válidos." });
        var sucursales = await _db.Sucursales.Where(x => x.IdEmpresa == EmpresaId() && x.Activo && request.SucursalesIds.Distinct().Contains(x.IdSucursal)).ToListAsync(cancellationToken);
        if (sucursales.Count != request.SucursalesIds.Distinct().Count()) return BadRequest(new { mensaje = "Una o más sucursales no son válidas." });
        if (request.IdSucursalPrincipal.HasValue && !request.SucursalesIds.Contains(request.IdSucursalPrincipal.Value)) return BadRequest(new { mensaje = "La sucursal principal debe estar asignada." });
        usuario.Username = username; usuario.Email = email; if (!string.IsNullOrWhiteSpace(request.Password)) usuario.PasswordHash = _passwordHasher.HashPassword(request.Password);
        _db.UsuariosRoles.RemoveRange(usuario.UsuariosRoles); foreach (var rol in roles) _db.UsuariosRoles.Add(new UsuarioRol { IdUsuario = idUsuario, IdRol = rol.IdRol });
        var asignaciones = await _db.UsuariosSucursales.Where(x => x.IdUsuario == idUsuario).ToListAsync(cancellationToken); _db.UsuariosSucursales.RemoveRange(asignaciones);
        foreach (var sucursal in sucursales) _db.UsuariosSucursales.Add(new UsuarioSucursal { IdUsuario = idUsuario, IdSucursal = sucursal.IdSucursal, EsPrincipal = request.IdSucursalPrincipal == sucursal.IdSucursal });
        usuario.FechaModificacion = DateTime.UtcNow; usuario.UsuarioModificacion = User.Identity?.Name; await _db.SaveChangesAsync(cancellationToken); return Ok();
    }

    [HttpPatch("usuarios/{idUsuario:int}/estado")]
    public async Task<IActionResult> CambiarEstadoUsuario(int idUsuario, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        var usuario = await _db.Usuarios.SingleOrDefaultAsync(x => x.IdUsuario == idUsuario, cancellationToken);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });
        if (!activo && usuario.IdUsuario == UsuarioId()) return BadRequest(new { mensaje = "No puedes desactivar tu propio usuario." });
        usuario.Activo = activo; usuario.FechaModificacion = DateTime.UtcNow; usuario.UsuarioModificacion = User.Identity?.Name; await _db.SaveChangesAsync(cancellationToken); return Ok();
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CrearRol([FromBody] GuardarRolConfiguracionRequest request, CancellationToken cancellationToken)
    {
        var nombre = request.NombreRol.Trim();
        if (await _db.Roles.AnyAsync(x => x.NombreRol == nombre, cancellationToken)) return Conflict(new { mensaje = "Ya existe un rol con ese nombre." });
        var permisos = await _db.Permisos.Where(x => x.Activo && request.PermisosIds.Distinct().Contains(x.IdPermiso)).ToListAsync(cancellationToken);
        if (permisos.Count != request.PermisosIds.Distinct().Count()) return BadRequest(new { mensaje = "Uno o más permisos no son válidos." });
        var rol = new Rol { NombreRol = nombre, Descripcion = request.Descripcion?.Trim(), Activo = true, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = User.Identity?.Name };
        _db.Roles.Add(rol); await _db.SaveChangesAsync(cancellationToken);
        foreach (var permiso in permisos) _db.RolesPermisos.Add(new RolPermiso { IdRol = rol.IdRol, IdPermiso = permiso.IdPermiso });
        await _db.SaveChangesAsync(cancellationToken); return CreatedAtAction(nameof(Obtener), new { idRol = rol.IdRol }, new { rol.IdRol, rol.NombreRol });
    }

    [HttpPut("roles/{idRol:int}")]
    public async Task<IActionResult> ActualizarRol(int idRol, [FromBody] GuardarRolConfiguracionRequest request, CancellationToken cancellationToken)
    {
        var rol = await _db.Roles.Include(x => x.RolesPermisos).SingleOrDefaultAsync(x => x.IdRol == idRol, cancellationToken);
        if (rol is null) return NotFound(new { mensaje = "Rol no encontrado." });
        var nombre = request.NombreRol.Trim();
        if (await _db.Roles.AnyAsync(x => x.IdRol != idRol && x.NombreRol == nombre, cancellationToken)) return Conflict(new { mensaje = "Ya existe un rol con ese nombre." });
        var permisos = await _db.Permisos.Where(x => x.Activo && request.PermisosIds.Distinct().Contains(x.IdPermiso)).ToListAsync(cancellationToken);
        if (permisos.Count != request.PermisosIds.Distinct().Count()) return BadRequest(new { mensaje = "Uno o más permisos no son válidos." });
        _db.RolesPermisos.RemoveRange(rol.RolesPermisos); rol.NombreRol = nombre; rol.Descripcion = request.Descripcion?.Trim(); rol.FechaModificacion = DateTime.UtcNow; rol.UsuarioModificacion = User.Identity?.Name;
        foreach (var permiso in permisos) _db.RolesPermisos.Add(new RolPermiso { IdRol = idRol, IdPermiso = permiso.IdPermiso });
        await _db.SaveChangesAsync(cancellationToken); return Ok();
    }

    [HttpPatch("roles/{idRol:int}/estado")]
    public async Task<IActionResult> CambiarEstadoRol(int idRol, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        var rol = await _db.Roles.Include(x => x.UsuariosRoles).SingleOrDefaultAsync(x => x.IdRol == idRol, cancellationToken);
        if (rol is null) return NotFound(new { mensaje = "Rol no encontrado." });
        if (!activo && rol.UsuariosRoles.Count > 0) return Conflict(new { mensaje = "No se puede desactivar un rol asignado a usuarios." });
        rol.Activo = activo; rol.FechaModificacion = DateTime.UtcNow; rol.UsuarioModificacion = User.Identity?.Name; await _db.SaveChangesAsync(cancellationToken); return Ok();
    }

    [HttpPost("series-fiscales")]
    public async Task<IActionResult> CrearSerieFiscal([FromBody] GuardarSerieFiscalRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId(); var tipo = request.TipoComprobante.Trim().ToUpperInvariant(); var serie = request.Serie.Trim().ToUpperInvariant();
        if (await _db.SeriesFiscales.AnyAsync(x => x.IdEmpresa == empresaId && x.TipoComprobante == tipo && x.Serie == serie, cancellationToken)) return Conflict(new { mensaje = "La serie ya existe para este tipo de comprobante." });
        if (request.IdSucursal.HasValue && !await _db.Sucursales.AnyAsync(x => x.IdSucursal == request.IdSucursal && x.IdEmpresa == empresaId, cancellationToken)) return BadRequest(new { mensaje = "La sucursal no es válida." });
        var item = new SerieFiscal { IdEmpresa = empresaId, IdSucursal = request.IdSucursal, TipoComprobante = tipo, Serie = serie, CorrelativoActual = request.CorrelativoActual, Activo = true, FechaCreacion = DateTime.UtcNow, UsuarioCreacion = User.Identity?.Name };
        _db.SeriesFiscales.Add(item); await _db.SaveChangesAsync(cancellationToken); return CreatedAtAction(nameof(Obtener), new { idSerieFiscal = item.IdSerieFiscal }, item);
    }

    [HttpPut("series-fiscales/{idSerieFiscal:int}")]
    public async Task<IActionResult> ActualizarSerieFiscal(int idSerieFiscal, [FromBody] GuardarSerieFiscalRequest request, CancellationToken cancellationToken)
    {
        var empresaId = EmpresaId(); var item = await _db.SeriesFiscales.SingleOrDefaultAsync(x => x.IdSerieFiscal == idSerieFiscal && x.IdEmpresa == empresaId, cancellationToken);
        if (item is null) return NotFound(new { mensaje = "Serie fiscal no encontrada." });
        var tipo = request.TipoComprobante.Trim().ToUpperInvariant(); var serie = request.Serie.Trim().ToUpperInvariant();
        if (await _db.SeriesFiscales.AnyAsync(x => x.IdSerieFiscal != idSerieFiscal && x.IdEmpresa == empresaId && x.TipoComprobante == tipo && x.Serie == serie, cancellationToken)) return Conflict(new { mensaje = "La serie ya existe para este tipo de comprobante." });
        var mayorEmitido = await _db.Comprobantes.Where(x => x.IdEmpresa == empresaId && x.TipoComprobante == item.TipoComprobante && x.SerieFiscal == item.Serie && x.CorrelativoFiscal.HasValue).MaxAsync(x => (long?)x.CorrelativoFiscal, cancellationToken) ?? 0;
        if (request.CorrelativoActual <= mayorEmitido) return Conflict(new { mensaje = "El correlativo debe ser mayor que los comprobantes ya emitidos." });
        if (request.IdSucursal.HasValue && !await _db.Sucursales.AnyAsync(x => x.IdSucursal == request.IdSucursal && x.IdEmpresa == empresaId, cancellationToken)) return BadRequest(new { mensaje = "La sucursal no es válida." });
        item.TipoComprobante = tipo; item.Serie = serie; item.IdSucursal = request.IdSucursal; item.CorrelativoActual = request.CorrelativoActual; item.FechaModificacion = DateTime.UtcNow; item.UsuarioModificacion = User.Identity?.Name; await _db.SaveChangesAsync(cancellationToken); return Ok(item);
    }

    [HttpPatch("series-fiscales/{idSerieFiscal:int}/estado")]
    public async Task<IActionResult> CambiarEstadoSerieFiscal(int idSerieFiscal, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        var item = await _db.SeriesFiscales.SingleOrDefaultAsync(x => x.IdSerieFiscal == idSerieFiscal && x.IdEmpresa == EmpresaId(), cancellationToken);
        if (item is null) return NotFound(new { mensaje = "Serie fiscal no encontrada." });
        item.Activo = activo; item.FechaModificacion = DateTime.UtcNow; item.UsuarioModificacion = User.Identity?.Name; await _db.SaveChangesAsync(cancellationToken); return Ok(item);
    }

    private int EmpresaId() => int.TryParse(User.FindFirstValue("IdEmpresa"), out var id) && id > 0 ? id : throw new UnauthorizedAccessException("El token no contiene una empresa válida.");
    private int UsuarioId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : throw new UnauthorizedAccessException("El token no contiene un usuario válido.");
}
