using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _baseStoragePath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(string baseStoragePath, ILogger<LocalFileStorageService> logger)
        {
            if (string.IsNullOrWhiteSpace(baseStoragePath))
            {
                throw new ArgumentException("Base storage path cannot be null or empty.", nameof(baseStoragePath));
            }

            _baseStoragePath = Path.GetFullPath(baseStoragePath);
            _logger = logger;

            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }
        }

        public async Task<string> GuardarArchivoAsync(string contenedor, string nombreArchivo, Stream contenido, string contentType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(contenedor)) throw new ArgumentException("Contenedor cannot be null or empty.", nameof(contenedor));
            if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("NombreArchivo cannot be null or empty.", nameof(nombreArchivo));
            if (contenido == null) throw new ArgumentNullException(nameof(contenido));

            ValidatePathTraversal(contenedor);
            ValidatePathTraversal(nombreArchivo);

            // La storageKey logica será: {contenedor}/{nombreArchivo}
            string storageKey = $"{contenedor}/{nombreArchivo}".Replace("\\", "/");

            string physicalPath = GetPhysicalPath(storageKey);

            string? directoryPath = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                if (contenido.CanSeek)
                {
                    contenido.Position = 0;
                }
                
                await contenido.CopyToAsync(fileStream, cancellationToken);
            }

            _logger.LogInformation("Archivo guardado físicamente en {PhysicalPath} con StorageKey {StorageKey}", physicalPath, storageKey);

            return storageKey;
        }

        public async Task<bool> IntentarCrearArchivoAsync(
            string contenedor,
            string nombreArchivo,
            Stream contenido,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(contenedor)) throw new ArgumentException("Contenedor cannot be null or empty.", nameof(contenedor));
            if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("NombreArchivo cannot be null or empty.", nameof(nombreArchivo));
            if (contenido == null) throw new ArgumentNullException(nameof(contenido));

            ValidatePathTraversal(contenedor);
            ValidatePathTraversal(nombreArchivo);

            var storageKey = $"{contenedor}/{nombreArchivo}".Replace("\\", "/");
            var physicalPath = GetPhysicalPath(storageKey);
            var directoryPath = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var temporaryPath = physicalPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                await using var fileStream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    useAsync: true);

                if (contenido.CanSeek)
                    contenido.Position = 0;

                await contenido.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
                fileStream.Close();

                // El rename dentro del mismo volumen publica el archivo completo de forma
                // atómica. overwrite:false convierte la colisión en un perdedor seguro.
                File.Move(temporaryPath, physicalPath, overwrite: false);
                _logger.LogInformation(
                    "Archivo creado sin sobrescritura en {PhysicalPath} con StorageKey {StorageKey}",
                    physicalPath,
                    storageKey);
                return true;
            }
            catch (IOException) when (File.Exists(physicalPath))
            {
                _logger.LogInformation(
                    "No se creó {StorageKey}: otro proceso ya materializó el archivo",
                    storageKey);
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                catch (IOException cleanupException)
                {
                    _logger.LogWarning(cleanupException, "No se pudo limpiar el temporal {TemporaryPath}", temporaryPath);
                }
            }
        }

        public Task<Stream?> ObtenerArchivoAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("StorageKey cannot be null or empty.", nameof(storageKey));

            ValidatePathTraversal(storageKey);

            string physicalPath = GetPhysicalPath(storageKey);

            if (!File.Exists(physicalPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return Task.FromResult<Stream?>(stream);
        }

        public Task<bool> ExisteArchivoAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("StorageKey cannot be null or empty.", nameof(storageKey));

            ValidatePathTraversal(storageKey);

            string physicalPath = GetPhysicalPath(storageKey);

            return Task.FromResult(File.Exists(physicalPath));
        }

        public Task EliminarArchivoAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("StorageKey cannot be null or empty.", nameof(storageKey));

            ValidatePathTraversal(storageKey);

            string physicalPath = GetPhysicalPath(storageKey);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("Archivo con StorageKey {StorageKey} eliminado físicamente en {PhysicalPath}", storageKey, physicalPath);
            }

            return Task.CompletedTask;
        }

        private string GetPhysicalPath(string storageKey)
        {
            // Normalize slashes
            string normalizedKey = storageKey.Replace("/", Path.DirectorySeparatorChar.ToString())
                                             .Replace("\\", Path.DirectorySeparatorChar.ToString());

            // Combinar con el base path
            string fullPath = Path.GetFullPath(Path.Combine(_baseStoragePath, normalizedKey));

            // Doble validación de Path Traversal
            if (!fullPath.StartsWith(_baseStoragePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Intento de Path Traversal detectado para la key: {StorageKey}", storageKey);
                throw new InvalidOperationException("La ruta resultante está fuera del directorio base permitido (Path Traversal detectado).");
            }

            return fullPath;
        }

        private void ValidatePathTraversal(string input)
        {
            if (input.Contains("..") || Path.IsPathRooted(input) || input.StartsWith("/") || input.StartsWith("\\"))
            {
                throw new ArgumentException($"El parámetro contiene caracteres inválidos de navegación de ruta: {input}");
            }
        }
    }
}
