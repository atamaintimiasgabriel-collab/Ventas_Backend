using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Comun.Excepciones.Finanzas;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Financieros
{
    public class ObligacionCxCService : IObligacionCxCService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IObligacionCxCRepository _repository;
        private readonly IVentaRepository _ventaRepository;

        public ObligacionCxCService(
            ICurrentUserService currentUserService, 
            IObligacionCxCRepository repository,
            IVentaRepository ventaRepository)
        {
            _currentUserService = currentUserService;
            _repository = repository;
            _ventaRepository = ventaRepository;
        }

        public async Task<ObligacionCxC> RegistrarObligacionAsync(ObligacionCxC obligacion, CancellationToken cancellationToken)
        {
            await ValidarEmpresaAsync(obligacion.IdEmpresa, 0, cancellationToken);
            obligacion.FechaCreacion = DateTime.UtcNow;
            obligacion.UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA");
            obligacion.Estado = "PENDIENTE";
            await _repository.AgregarAsync(obligacion, cancellationToken);
            return obligacion;
        }

        public async Task<ObligacionCxC> ObtenerObligacionAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken)
        {
            await ValidarEmpresaAsync(idEmpresa, idObligacion, cancellationToken);
            var obl = await _repository.ObtenerPorIdAsync(idEmpresa, idObligacion, cancellationToken);
            if (obl == null) throw new Exception("La obligación no existe.");
            return obl;
        }

        public async Task<decimal> ConsultarSaldoAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken)
        {
            var obl = await ObtenerObligacionAsync(idEmpresa, idObligacion, cancellationToken);
            return obl.DeudaOriginal; // Simplified, Orquestador might adjust this later if needed, but per rule 19 Orquestador asks Service.
        }

        public async Task<bool> ValidarEstadoAsync(int idEmpresa, int idObligacion, string estado, CancellationToken cancellationToken)
        {
            var obl = await ObtenerObligacionAsync(idEmpresa, idObligacion, cancellationToken);
            return obl.Estado == estado;
        }

        public async Task<bool> ValidarEmpresaAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken)
        {
            var empId = (_currentUserService.IdEmpresa ?? 0);
            if (empId != idEmpresa) throw new Exception("La empresa no corresponde al contexto actual.");
            return true;
        }

        public async Task<bool> ValidarVencimientoAsync(int idEmpresa, int idObligacion, CancellationToken cancellationToken)
        {
            var obl = await ObtenerObligacionAsync(idEmpresa, idObligacion, cancellationToken);
            return obl.FechaVencimiento < DateTime.UtcNow;
        }

        public async Task<ObligacionCxC> GenerarObligacionDesdeVentaAsync(int idEmpresa, int idVenta, decimal montoTotal, CancellationToken cancellationToken)
        {
            await ValidarEmpresaAsync(idEmpresa, 0, cancellationToken);
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta, cancellationToken);
            if (venta == null || venta.IdEmpresa != idEmpresa) throw new Exception("Venta no válida.");
            
            var existing = await _repository.ObtenerPorVentaAsync(idEmpresa, idVenta, cancellationToken);
            if (existing != null) return existing;

            var obl = new ObligacionCxC
            {
                IdEmpresa = idEmpresa,
                IdCliente = venta.IdCliente,
                IdVentaOrigen = idVenta,
                Moneda = "PEN",
                DeudaOriginal = montoTotal,
                FechaVencimiento = DateTime.UtcNow,
                Estado = "PENDIENTE",
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacion = (_currentUserService.IdUsuario ?? "SISTEMA"),
                Cliente = null!,
                Empresa = null!
            };
            await _repository.AgregarAsync(obl, cancellationToken);
            return obl;
        }
    }
}
