namespace SistemaVentasMotorrepuestos.API.DTOs.Clientes
{
    /// <summary>
    /// DTO para filtros en búsqueda y listado de clientes
    /// </summary>
    public class ClienteFilterRequest
    {
        /// <summary>
        /// Búsqueda: aplica a NumeroDocumento, RazonSocial, NombreComercial, Nombres, Apellidos, Email
        /// </summary>
        public string? Buscar { get; set; }

        /// <summary>
        /// Filtro por tipo de persona: 'N' = Natural, 'J' = Jurídica
        /// </summary>
        public string? TipoPersona { get; set; }

        /// <summary>
        /// Filtro por tipo de documento: DNI, RUC, CE, PAS, OTR
        /// </summary>
        public string? TipoDocumento { get; set; }

        /// <summary>
        /// Filtro por estado: true (Activo), false (Inactivo), null = todos
        /// </summary>
        public bool? Activo { get; set; } // null = todos; los consumidores que requieran activos lo indican explícitamente

        /// <summary>
        /// Paginación: número de página (base 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Paginación: tamaño de página
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
