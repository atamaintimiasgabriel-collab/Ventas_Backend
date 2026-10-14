using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SistemaVentasMotorrepuestos.API.Modelos;
using SistemaVentasMotorrepuestos.API.Modelos.DTOs.Facturacion;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces;
using SistemaVentasMotorrepuestos.API.Servicios.Interfaces.Facturacion;

namespace SistemaVentasMotorrepuestos.API.Servicios.Implementaciones.Facturacion
{
    public class ComprobanteDeliveryService : IComprobanteDeliveryService
    {
        private const string CanalEmail = "EMAIL";
        private const string CanalWhatsApp = "WHATSAPP";

        private readonly IEmailDeliveryAdapter _emailAdapter;
        private readonly IWhatsAppDeliveryAdapter _whatsAppAdapter;

        public ComprobanteDeliveryService(
            IEmailDeliveryAdapter emailAdapter,
            IWhatsAppDeliveryAdapter whatsAppAdapter)
        {
            _emailAdapter = emailAdapter;
            _whatsAppAdapter = whatsAppAdapter;
        }

        public async Task<ComprobanteDeliveryResultDTO> EnviarAsync(
            Comprobante comprobante,
            ComprobanteDeliveryRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (comprobante is null)
                return Error(string.Empty, "COMPROBANTE_INVALIDO", "El comprobante es obligatorio.");

            if (request is null)
                return Error(string.Empty, "REQUEST_INVALIDO", "La solicitud de entrega es obligatoria.");

            var canal = NormalizarCanal(request.Canal);

            if (request.IdEmpresa <= 0)
                return Error(canal, "EMPRESA_INVALIDA", "IdEmpresa debe ser mayor que cero.");

            if (request.IdEmpresa != comprobante.IdEmpresa)
                return Error(canal, "TENANT_MISMATCH", "El comprobante no pertenece a la empresa solicitante.");

            if (request.IdComprobante == Guid.Empty || request.IdComprobante != comprobante.IdComprobante)
                return Error(canal, "COMPROBANTE_INVALIDO", "La identificación del comprobante no coincide.");

            if (canal != CanalEmail && canal != CanalWhatsApp)
                return Error(canal, "CANAL_NO_SOPORTADO", "El canal solicitado no está soportado.");

            var destinoNormalizado = canal == CanalEmail
                ? NormalizarEmail(request.Destino)
                : NormalizarWhatsApp(request.Destino);

            if (destinoNormalizado is null)
                return Error(canal, "DESTINATARIO_INVALIDO", "El destinatario no tiene un formato válido.");

            var artefactos = ValidarYCopiarArtefactos(comprobante, request.Artefactos);
            if (artefactos is null)
                return Error(canal, "ARTEFACTO_INVALIDO", "Las referencias de artefactos no son válidas.");

            var solicitudAdapter = new ComprobanteDeliveryRequestDTO
            {
                IdEmpresa = request.IdEmpresa,
                IdComprobante = request.IdComprobante,
                Canal = canal,
                Destino = destinoNormalizado,
                Asunto = request.Asunto,
                Mensaje = request.Mensaje,
                Artefactos = artefactos
            };

            try
            {
                return canal == CanalEmail
                    ? await _emailAdapter.EnviarAsync(solicitudAdapter, cancellationToken)
                    : await _whatsAppAdapter.EnviarAsync(solicitudAdapter, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return Error(canal, "ADAPTER_ERROR", "El canal no pudo completar la entrega.");
            }
        }

        private static string NormalizarCanal(string? canal) =>
            canal?.Trim().ToUpperInvariant() ?? string.Empty;

        private static string? NormalizarEmail(string? destino)
        {
            var valor = destino?.Trim();
            return MailAddress.TryCreate(valor, out var email) &&
                   string.Equals(email.Address, valor, StringComparison.OrdinalIgnoreCase)
                ? email.Address
                : null;
        }

        private static string? NormalizarWhatsApp(string? destino)
        {
            if (string.IsNullOrWhiteSpace(destino)) return null;

            var valor = destino.Trim();
            if (valor.Any(c => !char.IsDigit(c) && c != '+' && c != ' ' && c != '-' && c != '(' && c != ')'))
                return null;

            var caracteres = valor
                .Where(c => char.IsDigit(c) || c == '+')
                .ToArray();
            var normalizado = new string(caracteres);

            if (normalizado.Count(c => c == '+') > 1 ||
                (normalizado.Contains('+') && !normalizado.StartsWith('+')))
                return null;

            var digitos = normalizado.TrimStart('+');
            return digitos.Length is >= 8 and <= 15 && digitos.All(char.IsDigit)
                ? normalizado
                : null;
        }

        private static IReadOnlyList<DeliveryArtifactReferenceDTO>? ValidarYCopiarArtefactos(
            Comprobante comprobante,
            IReadOnlyList<DeliveryArtifactReferenceDTO>? artefactos)
        {
            if (artefactos is null || artefactos.Count == 0) return null;

            var esperados = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["XML"] = comprobante.StorageKeyXml,
                ["CDR"] = comprobante.StorageKeyCdr,
                ["PDF"] = comprobante.StorageKeyPdf
            };
            var copia = new List<DeliveryArtifactReferenceDTO>(artefactos.Count);
            var tiposIncluidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var artefacto in artefactos)
            {
                var tipo = artefacto?.Tipo?.Trim().ToUpperInvariant();
                var key = artefacto?.StorageKey?.Trim();

                if (tipo is null || key is null ||
                    !esperados.TryGetValue(tipo, out var keyEsperada) ||
                    !tiposIncluidos.Add(tipo) ||
                    string.IsNullOrWhiteSpace(keyEsperada) ||
                    !string.Equals(key, keyEsperada, StringComparison.Ordinal) ||
                    EsRutaFisicaOReferenciaInsegura(key))
                    return null;

                copia.Add(new DeliveryArtifactReferenceDTO { Tipo = tipo, StorageKey = key });
            }

            return copia;
        }

        private static bool EsRutaFisicaOReferenciaInsegura(string storageKey) =>
            storageKey.Contains("..", StringComparison.Ordinal) ||
            storageKey.Contains(":\\", StringComparison.Ordinal) ||
            storageKey.StartsWith("\\", StringComparison.Ordinal) ||
            storageKey.StartsWith("/", StringComparison.Ordinal) ||
            Uri.TryCreate(storageKey, UriKind.Absolute, out _);

        private static ComprobanteDeliveryResultDTO Error(string canal, string codigo, string mensaje) =>
            new()
            {
                Exito = false,
                Canal = canal,
                Codigo = codigo,
                Mensaje = mensaje
            };
    }
}
