namespace SistemaVentasMotorrepuestos.API.DTOs.Autenticacion
{
    public class LoginResponseDto
    {
        public string Status { get; set; } = "Authenticated";
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<AuthorizedCompanyDto> Empresas { get; set; } = new();
    }

    public class AuthorizedCompanyDto
    {
        public int IdEmpresa { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
    }
}
