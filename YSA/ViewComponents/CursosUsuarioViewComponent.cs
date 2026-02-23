using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Web.Models.ViewModels;
using System.Security.Claims;

public class CursosUsuarioViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CursosUsuarioViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string version = "desktop")
    {
        var cursosComprados = new List<CursoViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int usuarioId))
            {
                // Obtenemos los cursos a los que el usuario tiene acceso
                var cursos = await _context.EstudianteCursos
                                           .Where(ec => ec.EstudianteId == usuarioId)
                                           .Include(ec => ec.Curso)
                                           .ThenInclude(c => c.CursoInstructores)
                                               .ThenInclude(ci => ci.Artista)
                                                   .ThenInclude(a => a.Usuario)
                                           .OrderByDescending(ec => ec.FechaAccesoOtorgado)
                                           .Take(5)
                                           .Select(ec => ec.Curso)
                                           .ToListAsync();

                // Mapeamos a un ViewModel
                cursosComprados = cursos.Select(c => new CursoViewModel
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    UrlImagen = c.UrlImagen,
                    DescripcionCorta = c.DescripcionCorta,
                    Precio = c.Precio
                }).ToList();
            }
        }

        if (version == "mobile")
            return View("_CursosUsuarioMobile", cursosComprados);
        else
            return View("Default", cursosComprados);
    }
}