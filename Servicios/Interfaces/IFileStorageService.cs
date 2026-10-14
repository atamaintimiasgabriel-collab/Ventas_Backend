using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaVentasMotorrepuestos.API.Servicios.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> GuardarArchivoAsync(string contenedor, string nombreArchivo, Stream contenido, string contentType, CancellationToken cancellationToken = default);
        Task<bool> IntentarCrearArchivoAsync(string contenedor, string nombreArchivo, Stream contenido, string contentType, CancellationToken cancellationToken = default);
        Task<Stream?> ObtenerArchivoAsync(string storageKey, CancellationToken cancellationToken = default);
        Task<bool> ExisteArchivoAsync(string storageKey, CancellationToken cancellationToken = default);
        Task EliminarArchivoAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
