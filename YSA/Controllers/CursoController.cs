using Microsoft.AspNetCore.Mvc;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using YSA.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
public class CursoController : Controller
{
    private readonly ICursoService _cursoService;
    private readonly IModuloService _moduloService;
    private readonly IEstudianteCursoService _estudianteCursoService;
    private readonly IPedidoService _pedidoService;
    private readonly IProgresoLeccionService _progresoLeccionService; // Nuevo
    private readonly IRecursoActividadService _recursoActividadService;
    private readonly IWebHostEnvironment _webHostEnvironment; // ¡NUEVO!

    public CursoController(ICursoService cursoService, IModuloService moduloService, IEstudianteCursoService estudianteCursoService, IPedidoService pedidoService, IProgresoLeccionService progresoLeccionService, IRecursoActividadService recursoActividadService, IWebHostEnvironment webHostEnvironment)
    {
        _cursoService = cursoService;
        _moduloService = moduloService;
        _estudianteCursoService = estudianteCursoService;
        _pedidoService = pedidoService;
        _progresoLeccionService = progresoLeccionService;
        _recursoActividadService = recursoActividadService;
        _webHostEnvironment = webHostEnvironment;
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
        int idUsuario = 0; // Inicializar idUsuario para usarlo más tarde

        if (User.Identity.IsAuthenticated)
        {
            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(estudianteId) && int.TryParse(estudianteId, out idUsuario))
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
            // Si no tiene acceso, lo redirige al detalle público.
            return RedirectToAction("Detalle", new { id = id });
        }

        // 1. Obtener la entidad Curso completa para acceder a Módulos y Lecciones
        var curso = await _cursoService.ObtenerCursoPorIdAsync(id);

        if (curso == null)
        {
            return NotFound();
        }

        // 2. Recolectar todos los IDs de entidades (Curso, Módulos, Lecciones)
        var entidadIds = new HashSet<int>();
        entidadIds.Add(curso.Id); // ID del Curso

        if (curso.Modulos != null)
        {
            foreach (var modulo in curso.Modulos)
            {
                entidadIds.Add(modulo.Id); // IDs de Módulos
                if (modulo.Lecciones != null)
                {
                    foreach (var leccion in modulo.Lecciones)
                    {
                        entidadIds.Add(leccion.Id); // IDs de Lecciones
                    }
                }
            }
        }

        // 3. Obtener TODOS los recursos (incluyendo actividades) asociados a cualquiera de estos IDs
        var recursos = await _recursoActividadService.ObtenerRecursosPorEntidadesAsync(entidadIds);
        // ^^^^^^ Se usa la nueva función con la lista de IDs ^^^^^^

        // Inicializar la lista de actividades del ViewModel
        var actividadesViewModel = new List<RecursoActividadViewModel>();

        if (recursos != null && recursos.Any())
        {
            // 4. Filtrar y mapear solo las Actividades que requieren entrega o son de tipo "Actividad"
            foreach (var recurso in recursos.Where(r => r.TipoRecurso == "Actividad" || r.RequiereEntrega))
            {
                EntregaActividadViewModel? entregaEstudianteVm = null;

                // Ya tenemos 'idUsuario' de la parte superior del método, lo reutilizamos.
                if (recurso.RequiereEntrega && idUsuario > 0)
                {
                    // Obtener la entrega del estudiante para esta actividad específica
                    var entrega = await _recursoActividadService.ObtenerEntregaPorActividadYEstudianteAsync(recurso.Id, idUsuario);

                    if (entrega != null)
                    {
                        entregaEstudianteVm = new EntregaActividadViewModel
                        {
                            Id = entrega.Id,
                            UrlArchivoEntrega = entrega.UrlArchivoEntrega,
                            ComentarioEstudiante = entrega.ComentarioEstudiante,
                            FechaEntrega = entrega.FechaEntrega,
                            Estado = entrega.Estado,
                            Calificacion = entrega.Calificacion,
                            ObservacionInstructor = entrega.ObservacionInstructor,
                            FechaCalificacion = entrega.FechaCalificacion
                        };
                    }
                }

                actividadesViewModel.Add(new RecursoActividadViewModel
                {
                    Id = recurso.Id,
                    Titulo = recurso.Titulo,
                    Descripcion = recurso.Descripcion,
                    TipoRecurso = recurso.TipoRecurso,
                    Url = recurso.Url,
                    RequiereEntrega = recurso.RequiereEntrega,
                    EntregaEstudiante = entregaEstudianteVm
                });
            }
        }

        // 5. Obtener el resto de datos
        var resenas = await _cursoService.ObtenerResenasPorCursoAsync(id);
        var resenasViewModel = resenas?.Select(r => new ResenaViewModel
        {
            NombreUsuario = r.Estudiante?.Nombre,
            Calificacion = r.Calificacion,
            Comentario = r.Comentario,
            Fecha = r.FechaResena,
            EstudianteId = r.EstudianteId,
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
            FechaRespuesta = p.FechaRespuesta,
            EstudianteId = p.EstudianteId,
        }).ToList();

        var anuncios = await _cursoService.ObtenerAnunciosPorCursoAsync(id);
        var anunciosVIewModel = anuncios.Select(c => new AnuncioViewModel
        {
            Id = c.Id,
            Titulo = c.Titulo,
            Contenido = c.Contenido,
            CursoId = c.CursoId,
            FechaPublicacion = c.FechaPublicacion,
        }).ToList();

        // 6. Mapeo final al ViewModel
        var cursoViewModel = new CursoCompletoViewModel
        {
            Id = curso.Id,
            Titulo = curso.Titulo,
            DescripcionCorta = curso.DescripcionCorta,
            DescripcionLarga = curso.DescripcionLarga,
            Precio = curso.Precio,
            UrlImagen = curso.UrlImagen,
            TieneAcceso = tieneAcceso,
            EstaEnValidacion = estaEnValidacion,
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
            Actividades = actividadesViewModel, // Contiene actividades de Curso, Módulo y Lección

        };

        // Esta verificación es redundante si ya comprobamos 'curso == null' arriba, pero se mantiene por seguridad.
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
    [Authorize] // Solo estudiantes autenticados pueden entregar
    [HttpPost]
    public async Task<IActionResult> SubirEntregaActividad(int cursoId, int recursoActividadId, string comentario, IFormFile archivoEntrega)
    {
        var estudianteIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(estudianteIdString, out int idEstudiante) || archivoEntrega == null || archivoEntrega.Length == 0)
        {
            TempData["Error"] = "Error: La sesión expiró o no se adjuntó el archivo.";
            return RedirectToAction("VerCurso", new { id = cursoId, tab = "proyectos-tab" });
        }

        string? urlArchivo = null;
        string? urlArchivoAntiguo = null;

        try
        {
            // 1. Verificar si ya existe una entrega
            var entregaExistente = await _recursoActividadService.ObtenerEntregaPorActividadYEstudianteAsync(recursoActividadId, idEstudiante);

            if (entregaExistente != null && entregaExistente.Estado == "Calificado")
            {
                TempData["Error"] = "No se puede reentregar una actividad que ya ha sido calificada.";
                return RedirectToAction("VerCurso", new { id = cursoId, tab = "proyectos-tab" });
            }

            // --- LÓGICA DE ALMACENAMIENTO DE ARCHIVOS INTEGRADA ---
            const string BASE_FOLDER = "actividades/actividadcargada";
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, BASE_FOLDER);

            // Crear la carpeta si no existe
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generar nombre de archivo único
            string extension = Path.GetExtension(archivoEntrega.FileName);
            string fileName = $"entrega_{recursoActividadId}_{idEstudiante}_{DateTime.UtcNow.Ticks}{extension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Guardar el archivo físicamente
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await archivoEntrega.CopyToAsync(fileStream);
            }

            // URL relativa para guardar en la base de datos
            urlArchivo = $"/{BASE_FOLDER}/{fileName}";
            // --- FIN LÓGICA DE ALMACENAMIENTO DE ARCHIVOS ---

            bool resultado;
            if (entregaExistente == null)
            {
                // **CREACIÓN**
                resultado = await _recursoActividadService.CrearEntregaAsync(
                    recursoActividadId, idEstudiante, urlArchivo, comentario);
            }
            else
            {
                // **ACTUALIZACIÓN/REENTREGA**
                urlArchivoAntiguo = entregaExistente.UrlArchivoEntrega; // Guardar URL antigua antes de actualizar

                resultado = await _recursoActividadService.ActualizarEntregaExistenteAsync(
                    recursoActividadId, idEstudiante, urlArchivo, comentario);
            }

            if (resultado)
            {
                TempData["Success"] = "¡Tu entrega ha sido enviada/actualizada con éxito!";

                // Si fue actualización y fue exitosa, eliminamos el archivo antiguo
                if (entregaExistente != null && !string.IsNullOrEmpty(urlArchivoAntiguo))
                {
                    EliminarArchivoLocal(urlArchivoAntiguo, _webHostEnvironment);
                }
            }
            else
            {
                // Fallo en la base de datos: Eliminar el nuevo archivo subido.
                EliminarArchivoLocal(urlArchivo, _webHostEnvironment);
                TempData["Error"] = "Error al registrar la entrega en la base de datos.";
            }
        }
        catch (Exception ex)
        {
            // Error catastrófico: Si se subió el archivo pero falló la DB, hay que eliminarlo.
            if (!string.IsNullOrEmpty(urlArchivo))
            {
                EliminarArchivoLocal(urlArchivo, _webHostEnvironment);
            }
            // Loggear ex.Message
            TempData["Error"] = "Ocurrió un error inesperado al procesar la entrega.";
        }

        return RedirectToAction("VerCurso", new { id = cursoId, tab = "proyectos-tab" });
    }

    // Método auxiliar privado para manejar la eliminación de archivos de forma limpia
    private static void EliminarArchivoLocal(string urlArchivo, IWebHostEnvironment env)
    {
        if (string.IsNullOrWhiteSpace(urlArchivo)) return;

        try
        {
            string filePath = Path.Combine(env.WebRootPath, urlArchivo.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch (Exception)
        {
            // Opcional: Loggear el fallo de eliminación, pero no interrumpimos el flujo
        }
    }
}