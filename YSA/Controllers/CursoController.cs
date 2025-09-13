using Microsoft.AspNetCore.Mvc;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using YSA.Core.Entities;

public class CursoController : Controller
{
    private readonly ICursoService _cursoService;
    private readonly IModuloService _moduloService;
    private readonly IEstudianteCursoService _estudianteCursoService;
    private readonly IPedidoService _pedidoService;
    private readonly IProgresoLeccionService _progresoLeccionService; // Nuevo

    public CursoController(ICursoService cursoService, IModuloService moduloService, IEstudianteCursoService estudianteCursoService, IPedidoService pedidoService, IProgresoLeccionService progresoLeccionService)
    {
        _cursoService = cursoService;
        _moduloService = moduloService;
        _estudianteCursoService = estudianteCursoService;
        _pedidoService = pedidoService;
        _progresoLeccionService = progresoLeccionService;
    }

    public async Task<IActionResult> Index()
    {
        // Obtener todos los cursos y categorías una sola vez
        var todosLosCursos = await _cursoService.ObtenerTodosLosCursosAsync();
        var todasLasCategorias = await _cursoService.ObtenerTodasLasCategoriasAsync();

        // Lógica para cursos destacados y recomendados (sin cambios)
        var cursosDestacados = todosLosCursos.Where(c => c.EsDestacado).ToList();
        var cursosRecomendados = todosLosCursos.Where(c => c.EsRecomendado && !c.EsDestacado).ToList();

        // Mapear los cursos
        var mapearCursos = (List<YSA.Core.Entities.Curso> cursos) => cursos.Select(c => new CursoIndexViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            DescripcionCorta = c.DescripcionCorta,
            UrlImagen = c.UrlImagen,
            Precio = c.Precio,
            ListaCategorias = c.CursoCategorias?.Select(cc => cc.Categoria.NombreCategoria).ToList() ?? new List<string>(),
            Nivel = c.Nivel
        }).ToList();

        // Crea el ViewModel para la vista principal
        var viewModel = new CursosIndexViewModel
        {
            CursosDestacados = mapearCursos(cursosDestacados),
            CursosRecomendados = mapearCursos(cursosRecomendados),
            CategoriasDisponibles = todasLasCategorias.Select(cat => cat.NombreCategoria).ToList(),
            // No se necesitan los campos de paginación o la lista de cursos en el modelo inicial
        };

        return View(viewModel);
    }

    // Agrega este nuevo método a tu CursoController.cs
    [HttpGet]
    public async Task<IActionResult> ObtenerCursosJson(string categoria, string searchString, int page = 1, int? nivel = null)
    {
        const int pageSize = 8;

        var cursos = await _cursoService.ObtenerTodosLosCursosAsync();

        // Filtro por categoría
        if (!string.IsNullOrEmpty(categoria))
        {
            cursos = cursos.Where(c => c.CursoCategorias.Any(cc => cc.Categoria.NombreCategoria == categoria)).ToList();
        }

        // Filtro por nivel de dificultad
        if (nivel.HasValue)
        {
            cursos = cursos.Where(c => (int)c.Nivel == nivel.Value).ToList();
        }

        // Filtro por búsqueda
        if (!string.IsNullOrEmpty(searchString))
        {
            cursos = cursos.Where(c =>
                c.Titulo.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) ||
                c.DescripcionCorta.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }


        var cursosOrdenados = cursos.OrderByDescending(c => c.Id).ToList();
        var totalCursos = cursosOrdenados.Count();
        var totalPaginas = (int)Math.Ceiling(totalCursos / (double)pageSize);
        var cursosPaginados = cursosOrdenados.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Mapeamos los cursos a un DTO para enviar solo la información necesaria
        var cursosDto = cursosPaginados.Select(c => new
        {
            id = c.Id,
            titulo = c.Titulo,
            descripcionCorta = c.DescripcionCorta,
            urlImagen = c.UrlImagen,
            precio = c.Precio.ToString("C", System.Globalization.CultureInfo.CreateSpecificCulture("es-VE")),
            categorias = c.CursoCategorias?.Select(cc => cc.Categoria.NombreCategoria).ToList() ?? new List<string>(),
            nivel = c.Nivel.ToString()
        }).ToList();

        // Devolvemos un objeto JSON con los datos de los cursos y la paginación
        return Json(new { cursos = cursosDto, totalPaginas = totalPaginas, paginaActual = page });
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
    public async Task<IActionResult> VerCurso(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool tieneAcceso = false;
        bool estaEnValidacion = false;
        List<ProgresoLeccion> progresoLecciones = new List<ProgresoLeccion>();
        if (User.Identity.IsAuthenticated)
        {
            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(estudianteId) && int.TryParse(estudianteId, out int idUsuario))
            {
                tieneAcceso = await _estudianteCursoService.TieneAccesoAlCursoAsync(idUsuario, id);

                if (!tieneAcceso)
                {
                    estaEnValidacion = await _pedidoService.TienePedidoPendientePorCursoAsync(idUsuario, id);
                }
                else
                {
                    progresoLecciones = await _progresoLeccionService.GetProgresoLeccionesPorEstudianteYCursoAsync(idUsuario, id);
                }
            }
        }
        if (!tieneAcceso)
        {
            return RedirectToAction("Detalle", new { id = id });
        }
        var resenas = await _cursoService.ObtenerResenasPorCursoAsync(id);
        var resenasViewModel = resenas?.Select(r => new ResenaViewModel
        {
            NombreUsuario = r.Estudiante?.Nombre,
            Calificacion = r.Calificacion,
            Comentario = r.Comentario,
            Fecha = r.FechaResena
        }).ToList();

        var preguntas = await _cursoService.ObtenerPreguntasPorCursoAsync(id);
        var preguntasViewModel = preguntas?.Select(p => new PreguntaRespuestaViewModel
         {
             Id = p.Id,
             Pregunta = p.Pregunta,
             FechaPregunta = p.FechaPregunta,
             NombreEstudiante = p.Estudiante?.Nombre,
             Respuesta = p.Respuesta,
             NombreInstructor = p.Instructor?.NombreArtistico,
             FechaRespuesta = p.FechaRespuesta
         }).ToList();

        var anuncios = await _cursoService.ObtenerAnunciosPorCursoAsync(id);
        // 5. Mapea a ViewModels
        var anunciosVIewModel = anuncios.Select(c => new AnuncioViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Contenido = c.Contenido,
            CursoId = c.CursoId,
            FechaPublicacion = c.FechaPublicacion,
        }).ToList();

        var curso = await _cursoService.ObtenerCursoPorIdAsync(id);

        if (curso == null)
        {
            return NotFound();
        }

        var cursoViewModel = new CursoCompletoViewModel
        {
            Id = curso.Id,
            Titulo = curso.Titulo,
            DescripcionCorta = curso.DescripcionCorta,
            DescripcionLarga = curso.DescripcionLarga,
            Precio = curso.Precio,
            UrlImagen = curso.UrlImagen,
            TieneAcceso = tieneAcceso,
            EstaEnValidacion = false, // Necesitas obtener este estado de tu base de datos si aplica.
            Modulos = curso.Modulos?.Select(m => new ModuloConLeccionesViewModel
            {
                Id = m.Id,
                Titulo = m.Titulo,
                Lecciones = m.Lecciones?.Select(l => new LeccionViewModel
                {
                    Id = l.Id,
                    Titulo = l.Titulo,
                    UrlVideo = l.UrlVideo,
                    Contenido = l.Contenido,
                    CompletadaPorEstudiante = progresoLecciones.Any(pl => pl.LeccionId == l.Id && pl.Completado)
                }).ToList()
            }).ToList(),
            Resenas = resenasViewModel,
            Preguntas = preguntasViewModel,
            Anuncios = anunciosVIewModel,

        };
        if (cursoViewModel == null)
        {
            return NotFound();
        }

        return View("CursoEstudiante", cursoViewModel);
    }
    public async Task<IActionResult> CrearResena(int CursoId, int Calificacion, string Comentario)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(); // O redirige a la página de login
        }
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest("El ID de usuario no es un formato válido.");
        }

        var resena = new Resena
        {
            CursoId = CursoId,
            EstudianteId = userIdInt,
            Calificacion = Calificacion,
            Comentario = Comentario,
            FechaResena = DateTime.UtcNow
        };

        await _cursoService.CrearResenaAsync(resena);

        // Redirige de vuelta a la página del curso
        return RedirectToAction("VerCurso", "Curso", new { id = CursoId });
    }
    [HttpPost]
    public async Task<IActionResult> CrearPregunta(int cursoId, string pregunta)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId))
        {
            return Unauthorized();
        }

        await _cursoService.CrearPreguntaAsync(cursoId, estudianteId, pregunta);

        // Redirige de vuelta a la página del curso, en la pestaña de preguntas
        return RedirectToAction("VerCurso", "Curso", new { id = cursoId, tab = "preguntas-tab" });
    }
    public async Task<IActionResult> MisCursos()
    {
        var userIdString = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var cursos = await _cursoService.ObtenerCursosDelEstudianteAsync(userId);
        var cursosViewModel = cursos.Select(c => new CursoViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            UrlImagen = c.UrlImagen,
            DescripcionCorta = c.DescripcionCorta
        }).ToList();

        return View(cursosViewModel);
    }
    [HttpPost]
    public async Task<IActionResult> MarcarLeccionComoCompletada(int leccionId)
    {
        // Obtener el ID del usuario actual.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int estudianteId))
        {
            return Json(new { success = false, message = "Usuario no autenticado o ID inválido." });
        }

        try
        {
            // Llamar al servicio de progreso para marcar la lección como completada
            var resultado = await _progresoLeccionService.MarcarLeccionComoCompletadaAsync(leccionId, estudianteId);

            if (resultado.success)
            {
                return Json(new { success = true });
            }
            else
            {
                // Devolver un error si el servicio falla (ej. lección no encontrada)
                return Json(new { success = false, message = resultado.message });
            }
        }
        catch (Exception ex)
        {
            // Loggear el error para depuración
            return Json(new { success = false, message = "Ocurrió un error interno al procesar la solicitud." });
        }
    }
}