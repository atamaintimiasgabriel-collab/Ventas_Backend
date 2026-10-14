using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IOrquestadorFinanciero
    {
        Task RegistrarVentaContadoAsync(int idVenta, decimal monto, int idMedioPago, int? idCaja, int? idCuentaTesoreria, CancellationToken cancellationToken);
        Task RegistrarVentaCreditoAsync(int idVenta, decimal monto, DateTime fechaVencimiento, CancellationToken cancellationToken);
        Task RegistrarPagoAsync(int idCliente, decimal monto, int idMedioPago, CancellationToken cancellationToken);
        Task AplicarPagoAsync(int idPago, int idObligacion, decimal montoAplicado, CancellationToken cancellationToken);
        Task AplicarPagoMultipleAsync(int idPago, IEnumerable<int> idObligaciones, IEnumerable<decimal> montos, CancellationToken cancellationToken);
        Task RegistrarPagoEfectivoAsync(int idCliente, decimal monto, int idCaja, CancellationToken cancellationToken);
        Task RegistrarPagoElectronicoAsync(int idCliente, decimal monto, int idCuentaTesoreria, int idProveedor, string referenciaExterna, CancellationToken cancellationToken);
        Task ProcesarPagoDivididoAsync(int idVenta, decimal montoEfectivo, int idCaja, decimal montoElectronico, int idCuentaTesoreria, CancellationToken cancellationToken);
        Task AnularPagoAsync(int idPago, string motivo, CancellationToken cancellationToken);
        Task AbrirCajaAsync(int idCaja, decimal saldoInicial, CancellationToken cancellationToken);
        Task CerrarCajaAsync(int idAperturaCaja, decimal saldoContado, string observaciones, CancellationToken cancellationToken);
        Task RegistrarMovimientoCajaAsync(int idAperturaCaja, decimal monto, string concepto, string tipo, CancellationToken cancellationToken);
        Task RegistrarMovimientoTesoreriaAsync(int idCuentaTesoreria, decimal monto, string descripcion, string tipo, CancellationToken cancellationToken);
    }
}
