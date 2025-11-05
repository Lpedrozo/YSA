// Controllers/NotificacionesController.cs
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;
using YSA.Core.Services;

namespace YSA.Web.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly INotificacionService _notificacionService;
        private readonly UserManager<Usuario> _userManager;

        public NotificacionesController(
            INotificacionService notificacionService,
            UserManager<Usuario> userManager)
        {
            _notificacionService = notificacionService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> MarcarComoLeida([FromBody] MarcarComoLeidaRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
                return Unauthorized();

            await _notificacionService.MarcarComoLeidaAsync(request.Id, usuarioId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
                return Unauthorized();

            await _notificacionService.MarcarTodasComoLeidasAsync(usuarioId);
            return Ok();
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerCantidadNoLeidas()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
                return Json(0);

            var cantidad = await _notificacionService.ObtenerCantidadNoLeidasAsync(usuarioId);
            return Json(cantidad);
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
                return RedirectToAction("Login", "Cuenta");

            var notificaciones = await _notificacionService.ObtenerNotificacionesUsuarioAsync(usuarioId);
            return View(notificaciones);
        }
    }

    public class MarcarComoLeidaRequest
    {
        public int Id { get; set; }
    }
}