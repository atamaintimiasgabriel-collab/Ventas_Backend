using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SistemaVentasMotorrepuestos.API.Hubs;

[Authorize]
public sealed class ComprasHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var claim = Context.User?.FindFirst("id_empresa")?.Value;
        if (int.TryParse(claim, out var idEmpresa) && idEmpresa > 0)
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoEmpresa(idEmpresa));
        await base.OnConnectedAsync();
    }

    public static string GrupoEmpresa(int idEmpresa) => $"compras:empresa:{idEmpresa}";
}
