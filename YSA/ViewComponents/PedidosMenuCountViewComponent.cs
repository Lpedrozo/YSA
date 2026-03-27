// PedidosMenuCountViewComponent.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YSA.Core.Entities;
using YSA.Core.Services;

public class PedidosMenuCountViewComponent : ViewComponent
{
    private readonly IPedidoService _pedidoService;
    private readonly UserManager<Usuario> _userManager;

    public PedidosMenuCountViewComponent(IPedidoService pedidoService, UserManager<Usuario> userManager)
    {
        _pedidoService = pedidoService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(User as ClaimsPrincipal);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
        {
            return Content("0");
        }

        var pendientes = await _pedidoService.ObtenerPedidosPorUsuarioYEstadoAsync(usuarioId, "Pendiente");
        var validando = await _pedidoService.ObtenerPedidosPorUsuarioYEstadoAsync(usuarioId, "Validando");

        var total = pendientes.Count() + validando.Count();
        return Content(total.ToString());
    }
}