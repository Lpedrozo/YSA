using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Web.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Services;

public class NotificacionesMenuViewComponent : ViewComponent
{
    private readonly INotificacionService _notificacionService;
    private readonly UserManager<Usuario> _userManager;

    public NotificacionesMenuViewComponent(
        INotificacionService notificacionService,
        UserManager<Usuario> userManager)
    {
        _notificacionService = notificacionService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(User as ClaimsPrincipal);

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int usuarioId))
        {
            return View(new NotificacionMenuWrapperViewModel
            {
                Notificaciones = new List<NotificacionMenuViewModel>(),
                TotalNoLeidas = 0
            });
        }

        var notificaciones = await _notificacionService.ObtenerNotificacionesUsuarioAsync(usuarioId);
        var noLeidas = await _notificacionService.ObtenerCantidadNoLeidasAsync(usuarioId);

        // Tomar solo las últimas 5 notificaciones para el menú
        var notificacionesRecientes = notificaciones
            .Take(5)
            .Select(n => new NotificacionMenuViewModel
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                UrlDestino = n.UrlDestino ?? "#",
                Icono = n.TipoNotificacion.Icono ?? "bell",
                Color = n.TipoNotificacion.Color ?? "primary",
                FechaCreacion = n.FechaCreacion,
                EsLeida = n.EsLeida,
                TiempoTranscurrido = CalcularTiempoTranscurrido(n.FechaCreacion)
            })
            .ToList();

        var wrapper = new NotificacionMenuWrapperViewModel
        {
            Notificaciones = notificacionesRecientes,
            TotalNoLeidas = noLeidas
        };

        return View(wrapper);
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