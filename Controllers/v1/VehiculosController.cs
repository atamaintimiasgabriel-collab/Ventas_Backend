using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos;

namespace SistemaVentasMotorrepuestos.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class VehiculosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiculosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("marcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            try
            {
                var marcas = await _context.MarcasVehiculos
                    .Where(m => m.Activo)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new { idMarca = m.IdMarcaVehiculo, nombre = m.Nombre })
                    .ToListAsync();
                
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener marcas de vehículos", error = ex.Message });
            }
        }

        [HttpGet("marcas/{idMarca}/modelos")]
        public async Task<IActionResult> ObtenerModelos(int idMarca)
        {
            try
            {
                var modelos = await _context.ModelosVehiculos
                    .Where(m => m.Activo && m.IdMarcaVehiculo == idMarca)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new { idModelo = m.IdModeloVehiculo, nombre = m.Nombre })
                    .ToListAsync();
                
                return Ok(modelos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener modelos de vehículos", error = ex.Message });
            }
        }

        [HttpGet("modelos/{idModelo}/anios")]
        public async Task<IActionResult> ObtenerAnios(int idModelo)
        {
            try
            {
                // Un modelo puede tener varias versiones con AñoInicio y AñoFin
                var versiones = await _context.VersionesVehiculos
                    .Where(v => v.Activo && v.IdModeloVehiculo == idModelo)
                    .ToListAsync();

                var anios = new HashSet<int>();
                foreach (var v in versiones)
                {
                    if (v.AñoInicio.HasValue)
                    {
                        int fin = v.AñoFin ?? DateTime.Now.Year;
                        for (int i = v.AñoInicio.Value; i <= fin; i++)
                        {
                            anios.Add(i);
                        }
                    }
                }

                return Ok(anios.OrderByDescending(a => a));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener años del modelo", error = ex.Message });
            }
        }

        [HttpGet("modelos/{idModelo}/cilindradas")]
        public async Task<IActionResult> ObtenerCilindradas(int idModelo)
        {
            try
            {
                // La cilindrada está en ModeloVehiculo o en VersionVehiculo?
                // Según ModeloVehiculo.cs, tiene string? Cilindrada
                var modelo = await _context.ModelosVehiculos
                    .Where(m => m.Activo && m.IdModeloVehiculo == idModelo)
                    .Select(m => m.Cilindrada)
                    .FirstOrDefaultAsync();

                var cilindradas = new List<string>();
                if (!string.IsNullOrEmpty(modelo))
                {
                    cilindradas.Add(modelo);
                }
                
                return Ok(cilindradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener cilindradas", error = ex.Message });
            }
        }
    }
}
