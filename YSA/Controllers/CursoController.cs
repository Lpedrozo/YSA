using Microsoft.AspNetCore.Mvc;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

public class CursoController : Controller
{
    private readonly ICursoService _cursoService;
    private readonly IModuloService _moduloService;
    private readonly IEstudianteCursoService _estudianteCursoService;
    private readonly IPedidoService _pedidoService;

    public CursoController(ICursoService cursoService, IModuloService moduloService, IEstudianteCursoService estudianteCursoService, IPedidoService pedidoService)
    {
        _cursoService = cursoService;
        _moduloService = moduloService;
        _estudianteCursoService = estudianteCursoService;
        _pedidoService = pedidoService;
    }

    public async Task<IActionResult> Index()
    {
        var cursos = await _cursoService.ObtenerTodosLosCursosAsync();

        var cursosOrdenados = cursos.OrderByDescending(c => c.Id).ToList();

        var cursoViewModels = cursosOrdenados.Select(c => new CursoViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            DescripcionCorta = c.DescripcionCorta,
            DescripcionLarga = c.DescripcionLarga,
            UrlImagen = c.UrlImagen,
            Precio = c.Precio
        }).ToList();

        return View(cursoViewModels);
    }
    public async Task<IActionResult> Detalles(int id)
    {
        var curso = await _cursoService.ObtenerCursoPorIdAsync(id);
        if (curso == null)
        {
            return NotFound();
        }

        bool tieneAcceso = false;
        bool estaEnValidacion = false;

        if (User.Identity.IsAuthenticated)
        {
            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(estudianteId) && int.TryParse(estudianteId, out int idUsuario))
            {
                // Verifica si ya tiene acceso al curso
                tieneAcceso = await _estudianteCursoService.TieneAccesoAlCursoAsync(idUsuario, id);

                // Si no tiene acceso, verifica si hay un pedido en validación
                if (!tieneAcceso)
                {
                    estaEnValidacion = await _pedidoService.TienePedidoPendientePorCursoAsync(idUsuario, id);
                }
            }
        }

        var modulos = await _moduloService.ObtenerModulosPorCursoIdAsync(id);

        var cursoViewModel = new CursoCompletoViewModel
        {
            Id = curso.Id,
            Titulo = curso.Titulo,
            DescripcionLarga = curso.DescripcionLarga,
            UrlImagen = curso.UrlImagen,
            Precio = curso.Precio,
            TieneAcceso = tieneAcceso,
            EstaEnValidacion = estaEnValidacion, // Pasa el estado de validación
            Modulos = modulos.OrderBy(m => m.Orden).Select(m => new ModuloConLeccionesViewModel
            {
                Id = m.Id,
                Titulo = m.Titulo,
                Lecciones = m.Lecciones.OrderBy(l => l.Orden).Select(l => new LeccionViewModel
                {
                    Id = l.Id,
                    Titulo = l.Titulo,
                    Contenido = l.Contenido,
                    UrlVideo = l.UrlVideo
                }).ToList()
            }).ToList()
        };

        return View(cursoViewModel);
    }
}