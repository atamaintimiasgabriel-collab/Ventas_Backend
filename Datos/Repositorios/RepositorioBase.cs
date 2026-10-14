using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaVentasMotorrepuestos.API.Datos.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Datos.Repositorios
{
    public class RepositorioBase<T> : IRepositorioBase<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositorioBase(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> ObtenerPorIdAsync(object id, CancellationToken cancellationToken = default)
        {
            // FindAsync no soporta AsNoTracking nativamente, pero es el más eficiente para PK simples.
            // Para proteger el tracking, las lecturas pesadas se harán con ObtenerTodosAsync o BuscarAsync.
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().Where(predicado).ToListAsync(cancellationToken);
        }

        public async Task AgregarAsync(T entidad, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entidad, cancellationToken);
        }

        public void Actualizar(T entidad)
        {
            // EF Core rastrea la entidad y la marca como modificada.
            _dbSet.Update(entidad);
        }

        public void Eliminar(T entidad)
        {
            // Este método solo marca para eliminación. La capa de negocio o servicio decidirá si esto es un Soft Delete o Hard Delete.
            _dbSet.Remove(entidad);
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
