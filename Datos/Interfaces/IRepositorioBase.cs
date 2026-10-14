using System.Linq.Expressions;

namespace SistemaVentasMotorrepuestos.API.Datos.Interfaces
{
    public interface IRepositorioBase<T> where T : class
    {
        Task<T?> ObtenerPorIdAsync(object id, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        
        Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado, CancellationToken cancellationToken = default);
        
        Task AgregarAsync(T entidad, CancellationToken cancellationToken = default);
        
        void Actualizar(T entidad);
        
        void Eliminar(T entidad);
        
        // El guardado se expone aquí para que la capa superior (Servicio) decida cuándo hacer el commit de la transacción
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    }
}
