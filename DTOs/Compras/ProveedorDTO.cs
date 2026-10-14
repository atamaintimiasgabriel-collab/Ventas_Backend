using System;

namespace SistemaVentasMotorrepuestos.API.DTOs.Compras
{
    public class ProveedorDTO
    {
        public int IdProveedor { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TotalCompras { get; set; }
        public decimal TotalComprasMonto { get; set; }
    }

    public class ProveedorFiltroDTO
    {
        public string? Buscar { get; set; }
        public string? TipoDocumento { get; set; }
        public bool? Estado { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ProveedorPaginadoDTO
    {
        public IReadOnlyList<ProveedorDTO> Items { get; set; } = Array.Empty<ProveedorDTO>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProveedorEstadisticasDTO
    {
        public int Total { get; set; }
        public int Activos { get; set; }
        public int Inactivos { get; set; }
        public int ComprasRealizadas { get; set; }
        public decimal TotalCompras { get; set; }
    }

    public class CrearProveedorDTO
    {
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }

    public class ActualizarProveedorDTO : CrearProveedorDTO { }
}
