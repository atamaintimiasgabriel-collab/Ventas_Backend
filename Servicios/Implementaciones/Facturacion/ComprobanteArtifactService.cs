using System.Globalization;
using Microsoft.Extensions.Logging;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    public sealed class ComprobanteArtifactService : IComprobanteArtifactService
    {
        private readonly IXmlSerializerService _xmlSerializer;
        private readonly IFileStorageService _storage;
        private readonly ILogger<ComprobanteArtifactService> _logger;

        public ComprobanteArtifactService(
            IXmlSerializerService xmlSerializer,
            IFileStorageService storage,
            ILogger<ComprobanteArtifactService> logger)
        {
            _xmlSerializer = xmlSerializer;
            _storage = storage;
            _logger = logger;
        }

        public async Task<string> AsegurarXmlAsync(
            DocumentoElectronicoDTO documento,
            string? storageKeyXmlPersistida,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(documento);
            cancellationToken.ThrowIfCancellationRequested();

            await using var xmlStream = await _xmlSerializer.SerializarAXmlAsync(documento, cancellationToken);
            var xmlEsperado = await LeerBytesAsync(xmlStream, cancellationToken);
            var storageKeyEsperada = StorageKeyGenerator.GenerarLlave(
                documento.IdEmpresa.ToString(CultureInfo.InvariantCulture),
                documento.FechaEmision,
                documento.TipoComprobante,
                "xml",
                documento.Serie,
                documento.Correlativo.ToString("D8", CultureInfo.InvariantCulture),
                ".xml");

            if (!string.IsNullOrWhiteSpace(storageKeyXmlPersistida) &&
                !string.Equals(storageKeyXmlPersistida, storageKeyEsperada, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "La StorageKeyXml persistida no coincide con la identidad fiscal determinística del comprobante.");
            }

            if (await _storage.ExisteArchivoAsync(storageKeyEsperada, cancellationToken))
            {
                await ValidarContenidoExistenteAsync(storageKeyEsperada, xmlEsperado, cancellationToken);
                return storageKeyEsperada;
            }

            var separatorIndex = storageKeyEsperada.IndexOf('/');
            if (separatorIndex <= 0 || separatorIndex == storageKeyEsperada.Length - 1)
                throw new InvalidOperationException("La StorageKey XML generada no tiene un formato válido.");

            var contenedor = storageKeyEsperada[..separatorIndex];
            var nombreArchivo = storageKeyEsperada[(separatorIndex + 1)..];
            await using var contenido = new MemoryStream(xmlEsperado, writable: false);
            var creado = await _storage.IntentarCrearArchivoAsync(
                contenedor,
                nombreArchivo,
                contenido,
                "application/xml",
                cancellationToken);

            await ValidarContenidoExistenteAsync(storageKeyEsperada, xmlEsperado, cancellationToken);
            _logger.LogInformation(
                creado
                    ? "Artefacto XML creado para Empresa {IdEmpresa}, Comprobante {IdComprobante}, StorageKey {StorageKey}"
                    : "Artefacto XML concurrente reutilizado para Empresa {IdEmpresa}, Comprobante {IdComprobante}, StorageKey {StorageKey}",
                documento.IdEmpresa,
                documento.IdComprobante,
                storageKeyEsperada);

            return storageKeyEsperada;
        }

        private async Task ValidarContenidoExistenteAsync(
            string storageKey,
            byte[] xmlEsperado,
            CancellationToken cancellationToken)
        {
            await using var existente = await _storage.ObtenerArchivoAsync(storageKey, cancellationToken)
                ?? throw new InvalidOperationException("El artefacto XML no pudo abrirse después de confirmar su existencia.");
            var bytesExistentes = await LeerBytesAsync(existente, cancellationToken);

            if (!bytesExistentes.AsSpan().SequenceEqual(xmlEsperado))
                throw new InvalidOperationException(
                    "Conflicto de artefacto XML: existe contenido diferente bajo la misma identidad fiscal.");
        }

        private static async Task<byte[]> LeerBytesAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            return buffer.ToArray();
        }
    }
}
