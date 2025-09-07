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

    public async Task<IActionResult> Index(string categoria, string searchString, int page = 1)
    {
        // Define la cantidad de cursos por página
        const int pageSize = 8;

        // 1. Obtiene todos los cursos y categorías
        var todosLosCursos = await _cursoService.ObtenerTodosLosCursosAsync();
        var todasLasCategorias = await _cursoService.ObtenerTodasLasCategoriasAsync();

        // 2. Filtra por categoría si se seleccionó una
        if (!string.IsNullOrEmpty(categoria))
        {
            todosLosCursos = todosLosCursos.Where(c => c.CursoCategorias.Any(cc => cc.Categoria.NombreCategoria == categoria)).ToList();
        }

        // 2.1. Filtra por cadena de búsqueda si se proporcionó una
        if (!string.IsNullOrEmpty(searchString))
        {
            todosLosCursos = todosLosCursos.Where(c =>
                c.Titulo.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) ||
                c.DescripcionCorta.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 3. Ordena los cursos
        var cursosOrdenados = todosLosCursos.OrderByDescending(c => c.Id).ToList();

        // 4. Implementa la paginación
        var totalCursos = cursosOrdenados.Count();
        var totalPaginas = (int)Math.Ceiling(totalCursos / (double)pageSize);
        var cursosPaginados = cursosOrdenados.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // 5. Mapea a ViewModels
        var cursosParaVista = cursosPaginados.Select(c => new CursoIndexViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            DescripcionCorta = c.DescripcionCorta,
            UrlImagen = c.UrlImagen,
            Precio = c.Precio,
            ListaCategorias = c.CursoCategorias?.Select(cc => cc.Categoria.NombreCategoria).ToList() ?? new List<string>()
        }).ToList();

        // 6. Crea el ViewModel completo
        var viewModel = new CursosIndexViewModel
        {
            Cursos = cursosParaVista,
            CategoriasDisponibles = todasLasCategorias.Select(cat => cat.NombreCategoria).ToList(),
            PaginaActual = page,
            TotalPaginas = totalPaginas,
            CategoriaActual = categoria
        };

        return View(viewModel);
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
            DescripcionCorta = curso.DescripcionCorta,
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