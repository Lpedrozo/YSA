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

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cursosComprados = new List<CursoViewModel>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Obtenemos los cursos a los que el usuario tiene acceso
                var cursos = await _context.EstudianteCursos
                                         .Where(ec => ec.EstudianteId == int.Parse(userId))
                                         .Include(ec => ec.Curso)
                                         .ThenInclude(c => c.Instructor) // Opcional, si quieres mostrar el instructor
                                         .OrderByDescending(ec => ec.FechaAccesoOtorgado)
                                         .Take(5) // Limita a los 5 cursos más recientes, como en la imagen de Udemy
                                         .Select(ec => ec.Curso)
                                         .ToListAsync();

                // Mapeamos a un ViewModel para la vista
                cursosComprados = cursos.Select(c => new CursoViewModel
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    UrlImagen = c.UrlImagen,
                }).ToList();
            }
        }
        return View(cursosComprados);
    }
}