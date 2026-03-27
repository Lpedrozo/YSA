using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Web.Models.ViewModels;
using System.Security.Claims;
using YSA.Core.Services;

namespace YSA.Web.ViewComponents
{
    public class PedidosMenuViewComponent : ViewComponent
    {
        private readonly IPedidoService _pedidoService;
        private readonly UserManager<Usuario> _userManager;

        public PedidosMenuViewComponent(
            IPedidoService pedidoService,
            UserManager<Usuario> userManager)
        {
            _pedidoService = pedidoService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string version = "desktop")
        {
            var userId = _userManager.GetUserId(User as ClaimsPrincipal);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
            {
                return View("Default", new PedidosMenuWrapperViewModel
                {
                    PedidosPendientes = new List<PedidosMenuViewModel>(),
                    PedidosValidando = new List<PedidosMenuViewModel>(),
                    TotalActivos = 0
                });
            }

            // Obtener pedidos pendientes y en validación
            var pedidosPendientes = await _pedidoService.ObtenerPedidosPorUsuarioYEstadoAsync(usuarioId, "Pendiente");
            var pedidosValidando = await _pedidoService.ObtenerPedidosPorUsuarioYEstadoAsync(usuarioId, "Validando");

            var pendientesVM = pedidosPendientes.Select(p => new PedidosMenuViewModel
            {
                Id = p.Id,
                FechaPedido = p.FechaPedido,
                Total = p.Total,
                Estado = p.Estado,
                CursoTitulo = p.PedidoItems.FirstOrDefault()?.VentaItem?.Curso?.Titulo ?? "Curso",
                CursoId = p.PedidoItems.FirstOrDefault()?.VentaItem?.CursoId ?? 0,
                TiempoTranscurrido = CalcularTiempoTranscurrido(p.FechaPedido)
            }).ToList();

            var validandoVM = pedidosValidando.Select(p => new PedidosMenuViewModel
            {
                Id = p.Id,
                FechaPedido = p.FechaPedido,
                Total = p.Total,
                Estado = p.Estado,
                CursoTitulo = p.PedidoItems.FirstOrDefault()?.VentaItem?.Curso?.Titulo ?? "Curso",
                CursoId = p.PedidoItems.FirstOrDefault()?.VentaItem?.CursoId ?? 0,
                TiempoTranscurrido = CalcularTiempoTranscurrido(p.FechaPedido)
            }).ToList();

            var wrapper = new PedidosMenuWrapperViewModel
            {
                PedidosPendientes = pendientesVM,
                PedidosValidando = validandoVM,
                TotalActivos = pendientesVM.Count + validandoVM.Count
            };

            if (version == "mobile")
                return View("_PedidosMenuMobile", wrapper);
            else
                return View("Default", wrapper);
        }

        private string CalcularTiempoTranscurrido(DateTime fechaCreacion)
        {
            var diferencia = DateTime.UtcNow - fechaCreacion;

            if (diferencia.TotalMinutes < 1)
                return "Ahora mismo";
            if (diferencia.TotalMinutes < 60)
                return $"{(int)diferencia.TotalMinutes} min";
            if (diferencia.TotalHours < 24)
                return $"{(int)diferencia.TotalHours} h";
            if (diferencia.TotalDays < 30)
                return $"{(int)diferencia.TotalDays} d";

            return fechaCreacion.ToString("dd/MM/yyyy");
        }
    }
}