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
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Interfaces;
public class CursoController : Controller
{
    private readonly ICursoService _cursoService;
    private readonly IModuloService _moduloService;
    private readonly IEstudianteCursoService _estudianteCursoService;
    private readonly IPedidoService _pedidoService;
    private readonly IProgresoLeccionService _progresoLeccionService;
    private readonly IRecursoActividadService _recursoActividadService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<Usuario> _userManager;
    private readonly IVentaItemService _ventaItemService;
    private readonly IEmailService _emailService; // NUEVO

    public CursoController(
        ICursoService cursoService,
        IModuloService moduloService,
        IEstudianteCursoService estudianteCursoService,
        IPedidoService pedidoService,
        IProgresoLeccionService progresoLeccionService,
        IRecursoActividadService recursoActividadService,
        IWebHostEnvironment webHostEnvironment,
        UserManager<Usuario> userManager,
        IVentaItemService ventaItemService,
        IEmailService emailService) // NUEVO PARÁMETRO
    {
        _cursoService = cursoService;
        _moduloService = moduloService;
        _estudianteCursoService = estudianteCursoService;
        _pedidoService = pedidoService;
        _progresoLeccionService = progresoLeccionService;
        _recursoActividadService = recursoActividadService;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
        _ventaItemService = ventaItemService;
        _emailService = emailService; // NUEVO
    }

    public async Task<IActionResult> Index(string categoria = null, string searchString = null,
        int? nivel = null, string precio = null, string orden = "recientes", int pagina = 1)
    {
        // Obtener todos los cursos y categorías
        var todosLosCursos = await _cursoService.ObtenerTodosLosCursosDigitalesAsync();
        var todasLasCategorias = await _cursoService.ObtenerTodasLasCategoriasAsync();

        // Obtener cursos destacados (siempre los mismos)
        var cursosDestacados = todosLosCursos
            .Where(c => c.EsDestacado)
            .OrderByDescending(c => c.Id)
            .Take(4)
            .ToList();

        // Mapear cursos
        Func<List<YSA.Core.Entities.Curso>, List<CursoIndexViewModel>> mapearCursos = (cursos) =>
            cursos.Select(c => new CursoIndexViewModel
            {
                Id = c.Id,
                Titulo = c.Titulo,
                DescripcionCorta = c.DescripcionCorta,
                UrlImagen = c.UrlImagen,
                Precio = c.Precio,
                ListaCategorias = c.CursoCategorias?.Select(cc => cc.Categoria.NombreCategoria).ToList() ?? new List<string>(),
                Nivel = c.Nivel
            }).ToList();

        // Crear ViewModel para la vista inicial
        var viewModel = new CursosIndexViewModel
        {
            CursosDestacados = mapearCursos(cursosDestacados),
            CategoriasDisponibles = todasLasCategorias.Select(cat => cat.NombreCategoria).ToList(),
            // NOTA: Los cursos normales NO se cargan aquí, se cargan vía AJAX
            Cursos = new List<CursoIndexViewModel>(), // Vacío porque se carga con AJAX
            PaginaActual = 1,
            TotalPaginas = 1,
            CategoriaActual = categoria
        };

        // Pasar parámetros a ViewBag para inicializar controles si vienen de URL
        ViewBag.Categoria = categoria;
        ViewBag.SearchString = searchString;
        ViewBag.Nivel = nivel;
        ViewBag.Precio = precio;
        ViewBag.Orden = orden;

        return View(viewModel);
    }    // Agrega este nuevo método a tu CursoController.cs
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> ObtenerCursosJson(string categoria = null, string searchString = null,
    int? nivel = null, string precio = null, string orden = "recientes", int page = 1)
    {
        try
        {
            const int pageSize = 9; // 3x3 en desktop

            // Obtener todos los cursos
            var todosLosCursos = await _cursoService.ObtenerTodosLosCursosDigitalesAsync();
            var cursosFiltrados = todosLosCursos.AsQueryable();

            // ===== APLICAR FILTROS =====

            // 1. Filtrar por categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                cursosFiltrados = cursosFiltrados.Where(c =>
                    c.CursoCategorias != null &&
                    c.CursoCategorias.Any(cc =>
                        cc.Categoria != null &&
                        cc.Categoria.NombreCategoria == categoria));
            }

            // 2. Filtrar por búsqueda de texto
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                cursosFiltrados = cursosFiltrados.Where(c =>
                    (c.Titulo != null && c.Titulo.ToLower().Contains(searchString)) ||
                    (c.DescripcionCorta != null && c.DescripcionCorta.ToLower().Contains(searchString)));
            }

            // 3. Filtrar por nivel
            if (nivel.HasValue)
            {
                cursosFiltrados = cursosFiltrados.Where(c => (int)c.Nivel == nivel.Value);
            }

            // 4. Filtrar por precio
            if (!string.IsNullOrEmpty(precio))
            {
                switch (precio.ToLower())
                {
                    case "gratis":
                        cursosFiltrados = cursosFiltrados.Where(c => c.Precio == 0);
                        break;
                    case "0-50":
                        cursosFiltrados = cursosFiltrados.Where(c => c.Precio > 0 && c.Precio <= 50);
                        break;
                    case "50-100":
                        cursosFiltrados = cursosFiltrados.Where(c => c.Precio > 50 && c.Precio <= 100);
                        break;
                    case "100+":
                        cursosFiltrados = cursosFiltrados.Where(c => c.Precio > 100);
                        break;
                }
            }

            // ===== APLICAR ORDEN =====
            switch (orden.ToLower())
            {
                case "destacados":
                    cursosFiltrados = cursosFiltrados.OrderByDescending(c => c.EsDestacado)
                                                     .ThenByDescending(c => c.Id);
                    break;
                case "precio-asc":
                    cursosFiltrados = cursosFiltrados.OrderBy(c => c.Precio)
                                                     .ThenByDescending(c => c.Id);
                    break;
                case "precio-desc":
                    cursosFiltrados = cursosFiltrados.OrderByDescending(c => c.Precio)
                                                     .ThenByDescending(c => c.Id);
                    break;
                case "recientes":
                default:
                    cursosFiltrados = cursosFiltrados.OrderByDescending(c => c.Id);
                    break;
            }

            // ===== PAGINACIÓN =====
            var totalCursos = cursosFiltrados.Count();
            var totalPaginas = (int)Math.Ceiling(totalCursos / (double)pageSize);

            // Validar página
            if (page < 1) page = 1;
            if (page > totalPaginas && totalPaginas > 0) page = totalPaginas;

            var cursosPaginados = cursosFiltrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ===== MAPEAR A DTO =====
            var cursosDto = cursosPaginados.Select(c => new
            {
                id = c.Id,
                titulo = c.Titulo ?? "Sin título",
                descripcionCorta = c.DescripcionCorta ?? "Sin descripción",
                urlImagen = c.UrlImagen,
                precio = c.Precio.ToString("C", CultureInfo.CreateSpecificCulture("es-VE")),
                categorias = c.CursoCategorias?
                    .Where(cc => cc.Categoria != null)
                    .Select(cc => cc.Categoria.NombreCategoria)
                    .ToList() ?? new List<string>(),
                nivel = c.Nivel.ToString()
            }).ToList();

            // ===== RETORNAR RESULTADO =====
            return Json(new
            {
                success = true,
                cursos = cursosDto,
                totalPaginas = totalPaginas,
                paginaActual = page,
                totalCursos = totalCursos
            });
        }
        catch (Exception ex)
        {
            // Loggear error
            return Json(new
            {
                success = false,
                message = "Error al cargar los cursos",
                error = ex.Message
            });
        }
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
        var recursosViewModel = new List<RecursoActividadViewModel>();

        if (recursos != null && recursos.Any())
        {
            foreach (var recurso in recursos)
            {
                EntregaActividadViewModel? entregaEstudianteVm = null;

                // Si requiere entrega, obtener la entrega del estudiante
                if (recurso.RequiereEntrega && idUsuario > 0)
                {
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

                var recursoVm = new RecursoActividadViewModel
                {
                    Id = recurso.Id,
                    Titulo = recurso.Titulo,
                    Descripcion = recurso.Descripcion,
                    TipoRecurso = recurso.TipoRecurso,
                    Url = recurso.Url,
                    RequiereEntrega = recurso.RequiereEntrega,
                    EntregaEstudiante = entregaEstudianteVm,
                    // Podemos agregar info de a qué entidad pertenece
                    TipoEntidad = recurso.TipoEntidad,
                    EntidadId = recurso.EntidadId
                };

                // Clasificar: si requiere entrega o es tipo "Actividad" va a actividades
                if (recurso.RequiereEntrega || recurso.TipoRecurso == "Actividad")
                {
                    actividadesViewModel.Add(recursoVm);
                }
                else
                {
                    recursosViewModel.Add(recursoVm);
                }
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
            Recursos = recursosViewModel,
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
    // GET: Detalle de Clase Presencial
    [HttpGet]
    public async Task<IActionResult> DetalleClasePresencial(int id)
    {
        var clase = await _cursoService.ObtenerClasePorIdAsync(id);
        if (clase == null || clase.Estado != "Programada")
        {
            return NotFound();
        }

        var curso = await _cursoService.ObtenerCursoPorIdAsync(clase.CursoId);
        if (curso == null)
        {
            return NotFound();
        }

        var vacantesDisponibles = clase.CapacidadMaxima - (clase.Inscripciones?.Count ?? 0);

        var viewModel = new ClasePresencialDetalleViewModel
        {
            ClaseId = clase.Id,
            CursoId = curso.Id,
            CursoTitulo = curso.Titulo,
            ClaseTitulo = clase.Titulo,
            ClaseDescripcion = clase.Descripcion,
            FechaHoraInicio = clase.FechaHoraInicio,
            FechaHoraFin = clase.FechaHoraFin,
            Lugar = clase.Lugar,
            CapacidadMaxima = clase.CapacidadMaxima,
            VacantesDisponibles = vacantesDisponibles,
            UrlImagen = curso.UrlImagen,
            Precio = curso.Precio,
            Estado = clase.Estado,
            UrlMeet = clase.UrlMeet,
            UsuarioLogueado = User.Identity.IsAuthenticated,
            PerfilCompleto = false,
            YaInscrito = false,
            TienePedidoPendiente = false
        };

        if (User.Identity.IsAuthenticated)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(usuarioId) && int.TryParse(usuarioId, out int idUsuario))
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                viewModel.PerfilCompleto = UsuarioTienePerfilCompleto(usuario);
                viewModel.YaInscrito = await _cursoService.GetInscripcionByClaseAndEstudianteAsync(clase.Id, idUsuario) != null;
                viewModel.TienePedidoPendiente = await _pedidoService.TienePedidoPendientePorCursoAsync(idUsuario, curso.Id);

                // Pasar datos del usuario para el modal
                viewModel.UsuarioNombre = usuario.Nombre;
                viewModel.UsuarioApellido = usuario.Apellido;
            }
        }

        return View(viewModel);
    }    // Método auxiliar para verificar si el usuario tiene el perfil completo
    private bool UsuarioTienePerfilCompleto(Usuario usuario)
    {
        return !string.IsNullOrEmpty(usuario.Cedula) &&
               !string.IsNullOrEmpty(usuario.WhatsApp) &&
               usuario.FechaNacimiento.HasValue;
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarCompraClasePresencial(int claseId, string metodoPago, string referenciaPago, IFormFile comprobanteArchivo)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId) || !int.TryParse(usuarioId, out int idUsuario))
        {
            return RedirectToAction("Login", "Cuenta");
        }

        // Verificar si el usuario tiene el perfil completo
        var usuario = await _userManager.FindByIdAsync(usuarioId);
        if (!UsuarioTienePerfilCompleto(usuario))
        {
            TempData["RedirectToProfile"] = true;
            TempData["Message"] = "Completa tus datos personales antes de inscribirte a una clase presencial.";
            return RedirectToAction("DetalleClasePresencial", new { id = claseId });
        }

        var clase = await _cursoService.ObtenerClasePorIdAsync(claseId);
        if (clase == null || clase.Estado != "Programada")
        {
            return NotFound();
        }

        var vacantes = clase.CapacidadMaxima - (clase.Inscripciones?.Count ?? 0);
        if (vacantes <= 0)
        {
            TempData["Error"] = "Lo sentimos, esta clase ya no tiene cupos disponibles.";
            return RedirectToAction("DetalleClasePresencial", new { id = claseId });
        }

        // Obtener el curso para saber el precio
        var curso = await _cursoService.ObtenerCursoPorIdAsync(clase.CursoId);
        bool esGratuito = curso.Precio == 0;

        // Lista de items comprados (para el correo del usuario)
        var itemsComprados = new List<string> { $"{clase.Titulo} - Clase presencial" };

        // ==================== CURSO GRATUITO ====================
        if (esGratuito)
        {
            try
            {
                // 1. Obtener o crear VentaItem para el curso
                var ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(clase.CursoId);
                if (ventaItem == null)
                {
                    await _ventaItemService.CrearVentaItemAsync("Curso", clase.CursoId, null, 0);
                    ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(clase.CursoId);
                }

                // 2. Crear pedido
                var pedido = await _pedidoService.CrearPedidoAsync(idUsuario, new List<int> { ventaItem.Id });

                // 3. Registrar pago gratuito
                var pago = new Pago
                {
                    PedidoId = pedido.Id,
                    MetodoPago = "Gratuito",
                    ReferenciaPago = "0000000000",
                    UrlComprobante = "/comprobantes/gratuito.jpg",
                    FechaPago = DateTime.UtcNow
                };
                await _pedidoService.RegistrarPagoAsync(pago);

                // 4. Aprobar pedido automáticamente
                await _pedidoService.AprobarPedidoYOtorgarAccesoAsync(pedido.Id);

                // 5. Inscribir al estudiante en la clase
                await _cursoService.InscribirEstudianteAClaseAsync(claseId, idUsuario);

                // 6. ENVIAR CORREO AL USUARIO (compra aprobada)
                await _emailService.EnviarCorreoCompraAprobadaAsync(
                    usuario.Email,
                    $"{usuario.Nombre} {usuario.Apellido}",
                    pedido.Id,
                    "clase presencial",
                    $"{clase.Titulo} - {curso.Titulo}"
                );

                // 7. ENVIAR NOTIFICACIÓN AL ADMINISTRADOR (inscripción gratuita)
                await _emailService.EnviarNotificacionAdminInscripcionGratuitaAsync(
                    $"{usuario.Nombre} {usuario.Apellido}",
                    usuario.Email,
                    clase.Titulo,
                    curso.Titulo,
                    clase.FechaHoraInicio,
                    clase.Lugar
                );

                TempData["Success"] = "¡Inscripción exitosa! La clase es gratuita, ya tienes tu cupo confirmado. Te hemos enviado un correo con los detalles.";
                return RedirectToAction("DetalleClasePresencial", new { id = claseId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al procesar tu inscripción gratuita. Intente de nuevo.";
                return RedirectToAction("DetalleClasePresencial", new { id = claseId });
            }
        }

        // ==================== CURSO DE PAGO ====================
        // Validar comprobante
        if (comprobanteArchivo == null || comprobanteArchivo.Length == 0)
        {
            TempData["Error"] = "Debe subir un comprobante de pago.";
            return RedirectToAction("DetalleClasePresencial", new { id = claseId });
        }

        try
        {
            // 1. Guardar comprobante
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(comprobanteArchivo.FileName)}";
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await comprobanteArchivo.CopyToAsync(stream);
            }
            var urlComprobante = $"/comprobantes/{fileName}";

            // 2. Obtener o crear VentaItem para el curso
            var ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(clase.CursoId);
            if (ventaItem == null)
            {
                await _ventaItemService.CrearVentaItemAsync("Curso", clase.CursoId, null, curso.Precio);
                ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(clase.CursoId);
            }

            // 3. Crear pedido
            var pedido = await _pedidoService.CrearPedidoAsync(idUsuario, new List<int> { ventaItem.Id });

            // 4. Registrar pago
            var pago = new Pago
            {
                PedidoId = pedido.Id,
                MetodoPago = metodoPago,
                ReferenciaPago = referenciaPago,
                UrlComprobante = urlComprobante,
                FechaPago = DateTime.UtcNow
            };
            await _pedidoService.RegistrarPagoAsync(pago);

            // 5. Actualizar estado del pedido a "Validando"
            await _pedidoService.ActualizarEstadoPedidoAsync(pedido.Id, "Validando");

            // 6. ENVIAR CORREO AL USUARIO (compra pendiente)
            await _emailService.EnviarCorreoCompraPendienteAsync(
                usuario.Email,
                $"{usuario.Nombre} {usuario.Apellido}",
                pedido.Id,
                pedido.Total,
                itemsComprados
            );

            // 7. ENVIAR NOTIFICACIÓN AL ADMINISTRADOR (pago pendiente)
            await _emailService.EnviarNotificacionAdminPagoPendienteAsync(
                $"{usuario.Nombre} {usuario.Apellido}",
                usuario.Email,
                clase.Titulo,
                curso.Titulo,
                curso.Precio,
                pedido.Id,
                urlComprobante
            );

            TempData["Success"] = "¡Pago registrado exitosamente! Tu inscripción está en proceso de validación. Te hemos enviado un correo con los detalles.";
            return RedirectToAction("DetalleClasePresencial", new { id = claseId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Ocurrió un error al procesar su pago. Intente de nuevo.";
            return RedirectToAction("DetalleClasePresencial", new { id = claseId });
        }
    }
}