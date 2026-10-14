using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones
{
    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _comprobanteRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly ISerieFiscalRepository _serieFiscalRepository;

        public ComprobanteService(
            IComprobanteRepository comprobanteRepository,
            IVentaRepository ventaRepository,
            ISerieFiscalRepository serieFiscalRepository)
        {
            _comprobanteRepository = comprobanteRepository;
            _ventaRepository = ventaRepository;
            _serieFiscalRepository = serieFiscalRepository;
        }

        public async Task<Comprobante> CrearComprobanteAsync(
            int idEmpresa, 
            int idVenta, 
            string tipoComprobante, 
            string serieFiscal, 
            string usuarioCreacion, 
            string idempotencyKey, 
            CancellationToken cancellationToken = default)
        {
            // 1. Validar Venta
            var venta = await _ventaRepository.ObtenerVentaPorIdAsync(idEmpresa, idVenta, cancellationToken);
            if (venta == null)
                throw new ArgumentException("La venta especificada no existe o no pertenece a la empresa.");

            // 2. Validar Serie Fiscal
            SerieFiscal? serie;
            if (string.IsNullOrWhiteSpace(serieFiscal))
            {
                serie = await _serieFiscalRepository.ObtenerSerieActivaAsync(idEmpresa, venta.IdSucursal, tipoComprobante, cancellationToken);
                if (serie == null)
                {
                    var globales = await _serieFiscalRepository.BuscarPorEmpresaAsync(idEmpresa, null, true, cancellationToken);
                    serie = globales.FirstOrDefault(s => s.IdSucursal == null && s.TipoComprobante == tipoComprobante);
                }
            }
            else
            {
                serie = await _serieFiscalRepository.BuscarSerieAsync(idEmpresa, tipoComprobante, serieFiscal, cancellationToken);
            }
            if (serie == null || !serie.Activo)
                throw new ArgumentException("La serie fiscal especificada no existe o no está activa.");

            // 3. Idempotencia Interna (Por Venta y Tipo de Comprobante)
            var existentes = await _comprobanteRepository.ObtenerPorVentaAsync(idVenta, idEmpresa, cancellationToken);
            var comprobanteActivo = existentes.FirstOrDefault(c => c.TipoComprobante == tipoComprobante && c.EstadoFiscal != "RECHAZADO" && c.EstadoFiscal != "ANULADO");
            
            if (comprobanteActivo != null)
            {
                // Si ya existe un comprobante válido para esta venta y tipo, lo retornamos (Idempotencia)
                return comprobanteActivo;
            }

            // 4. Crear nuevo Comprobante
            var nuevoComprobante = new Comprobante
            {
                IdComprobante = Guid.NewGuid(),
                IdEmpresa = idEmpresa,
                IdSucursal = venta.IdSucursal,
                IdVenta = idVenta,
                TipoComprobante = tipoComprobante,
                SerieFiscal = serie.Serie,
                CorrelativoFiscal = null, // Se asigna justo antes de enviar
                VersionNormativa = SunatFiscalValidator.VersionNormativa,
                TipoOperacionSunat = tipoComprobante is "01" or "03" or "FACTURA" or "BOLETA" ? "0101" : null,
                
                EstadoProcesamiento = "LIBRE",
                NumeroIntento = 0,
                FechaProximoIntento = DateTime.UtcNow,
                
                EstadoFiscal = "NO_ENVIADO",
                
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = usuarioCreacion
            };

            // 5. Registrar Auditoría Inicial
            var auditoria = new ComprobanteAuditoria
            {
                IdEmpresa = idEmpresa,
                IdComprobante = nuevoComprobante.IdComprobante,
                EstadoProcesamientoAnterior = null,
                EstadoProcesamientoNuevo = "LIBRE",
                EstadoFiscalAnterior = null,
                EstadoFiscalNuevo = "NO_ENVIADO",
                Evento = "CREACION",
                FechaEvento = DateTime.UtcNow,
                Detalle = "Comprobante creado exitosamente."
            };
            nuevoComprobante.Auditorias.Add(auditoria);

            await _comprobanteRepository.AgregarAsync(nuevoComprobante, cancellationToken);
            await _comprobanteRepository.GuardarCambiosAsync(cancellationToken);

            return nuevoComprobante;
        }

        public async Task<Comprobante?> ConsultarEstadoAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default)
        {
            return await _comprobanteRepository.ObtenerPorIdAsync(idComprobante, idEmpresa, cancellationToken);
        }

        public async Task<IEnumerable<ComprobanteAuditoria>> ConsultarHistorialAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default)
        {
            var comprobante = await _comprobanteRepository.ObtenerPorIdConAuditoriaAsync(idComprobante, idEmpresa, cancellationToken);
            return comprobante?.Auditorias ?? new List<ComprobanteAuditoria>();
        }

        public async Task SolicitarProcesamientoAsync(Guid idComprobante, int idEmpresa, CancellationToken cancellationToken = default)
        {
            var comprobante = await _comprobanteRepository.ObtenerPorIdAsync(idComprobante, idEmpresa, cancellationToken);
            if (comprobante == null)
                throw new ArgumentException("Comprobante no encontrado.");

            if (comprobante.EstadoProcesamiento == "FINALIZADO")
                throw new InvalidOperationException("El comprobante ya se encuentra finalizado.");

            // Recien creado: ya esta LIBRE y elegible para el worker; evita re-adjuntar la entidad rastreada.
            if (comprobante.EstadoProcesamiento == "LIBRE" && comprobante.EstadoFiscal == "NO_ENVIADO" && comprobante.NumeroIntento == 0)
                return;

            // Capturar estado anterior ANTES de sobrescribir
            var estadoAnterior = comprobante.EstadoProcesamiento;

            // Si estaba bloqueado o con un lease, forzamos su liberación para procesamiento inmediato
            comprobante.EstadoProcesamiento = "LIBRE";
            comprobante.FechaProximoIntento = DateTime.UtcNow;
            comprobante.WorkerId = null;
            comprobante.ExecutionToken = null;
            comprobante.LeaseHasta = null;

            var auditoria = new ComprobanteAuditoria
            {
                IdEmpresa = idEmpresa,
                IdComprobante = comprobante.IdComprobante,
                EstadoProcesamientoAnterior = estadoAnterior,   // estado real anterior
                EstadoProcesamientoNuevo = "LIBRE",
                EstadoFiscalAnterior = comprobante.EstadoFiscal,
                EstadoFiscalNuevo = comprobante.EstadoFiscal,
                Evento = "REPROCESAMIENTO_SOLICITADO",
                FechaEvento = DateTime.UtcNow,
                Detalle = "Se ha solicitado el reprocesamiento manual del comprobante."
            };

            comprobante.Auditorias.Add(auditoria);

            _comprobanteRepository.Actualizar(comprobante);
            await _comprobanteRepository.GuardarCambiosAsync(cancellationToken);
        }
    }
}
