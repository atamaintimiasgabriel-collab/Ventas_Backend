using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SistemaVentasMotorrepuestos.API.Hubs;

[Authorize]
public sealed class InventarioHub : Hub
{
    public Task UnirseAEmpresa(int idEmpresa)
    {
        var claim = Context.User?.FindFirst("id_empresa")?.Value;
        if (!int.TryParse(claim, out var empresa) || empresa != idEmpresa)
            throw new HubException("Empresa no autorizada.");
        return Groups.AddToGroupAsync(Context.ConnectionId, GrupoEmpresa(idEmpresa));
    }

    public static string GrupoEmpresa(int idEmpresa) => $"empresa:{idEmpresa}";
}
