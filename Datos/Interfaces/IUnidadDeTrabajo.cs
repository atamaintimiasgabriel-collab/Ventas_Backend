using Microsoft.EntityFrameworkCore.Storage;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IUnidadDeTrabajo : IDisposable
    {
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
        
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
