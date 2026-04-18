using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Enums;
using YSA.Core.Interfaces;
using YSA.Core.Services;
using YSA.Data.Repositories;
using YSA.Web.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YSA.Web.Controllers
{
    [Authorize(Roles = "Administrador, Asistente")]
    public class AdminController : Controller
    {
        private readonly ICursoService _cursoService;
        private readonly IEmailService _emailService;
        private readonly IPaqueteService _paqueteService;
        private readonly IModuloService _moduloService;
        private readonly ILeccionService _leccionService;
        private readonly IPedidoService _pedidoService;
        private readonly IVentaItemService _ventaItemService;
        private readonly IArtistaService _artistaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEventoService _eventoService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IProductoService _productoService;
        private readonly IRecursoActividadService _recursoActividadService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IArticuloService _articuloService; 
        private readonly IUsuarioService _usuarioService; 
        private readonly INotificacionService _notificacionService; 

        public AdminController(ICursoService cursoService, 
            IModuloService moduloService,
            IEmailService emailService,
            IPaqueteService paqueteService, 
            ILeccionService leccionService, 
            IPedidoService pedidoService,
            UserManager<Usuario> userManager, 
            IVentaItemService ventaItemService, 
            IArtistaService artistaService, 
            IEventoService eventoService, 
            IWebHostEnvironment hostingEnvironment, 
            IProductoService productoService,
            IRecursoActividadService recursoActividadService,
            IExchangeRateService exchangeRateService,
            IArticuloService articuloService,
            IUsuarioService usuarioService,
            INotificacionService notificacionService)
        {
            _cursoService = cursoService;
            _emailService = emailService;
            _paqueteService = paqueteService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
            _artistaService = artistaService;
            _eventoService = eventoService;
            _hostingEnvironment = hostingEnvironment;
            _productoService = productoService;
            _recursoActividadService = recursoActividadService;
            _exchangeRateService = exchangeRateService;
            _articuloService = articuloService;
            _usuarioService = usuarioService; 
            _notificacionService = notificacionService; 
        }

        [HttpGet]
        public async Task<IActionResult> HistorialTasas()
        {
            // 1. EL CONTROLADOR LLAMA AL SERVICIO
            var tasas = await _exchangeRateService.GetRateHistoryAsync();

            // 2. Continúa con la creación del ViewModel
            var model = new TasaBCVHistorialViewModel
            {
                Tasas = tasas
            };

            return View(model);
        }

        public async Task<IActionResult> Panel() // Lo hacemos ASÍNCRONO
        {
            var tasaBcv = await _exchangeRateService.GetTasaToday();
            var cursos = await _cursoService.GetTotalCursosAsync();
            var pedidos = await _pedidoService.GetPedidosPendientesAsync();
            var estudiantes = await _usuarioService.GetTotalEstudiantesAsync();

            var model = new DashboardViewModel
            {
                TotalCursos = cursos, 
                TotalEstudiantes = estudiantes,
                PedidosPendientes = pedidos, 
                TasaBCV = tasaBcv
            };
            return View(model);
        }

        // ----------------- Category Management Actions (CRUD) -----------------

        public async Task<IActionResult> GestionarCategorias()
        {
            var categorias = await _cursoService.ObtenerTodasLasCategoriasAsync();
            var viewModels = categorias.Select(c => new CategoriaViewModel
            {
                Id = c.Id,
                NombreCategoria = c.NombreCategoria
            }).ToList();
            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCategoriasJson()
        {
            var categorias = await _cursoService.ObtenerTodasLasCategoriasAsync();
            return Json(categorias.Select(c => new { id = c.Id, nombreCategoria = c.NombreCategoria }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria([FromBody] CategoriaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    )
                });
            }

            var categoria = new Categoria { NombreCategoria = viewModel.NombreCategoria };
            await _cursoService.CrearCategoriaAsync(categoria);
            return Json(new { success = true, message = "Categoría creada con éxito." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCategoria([FromBody] CategoriaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    )
                });
            }

            var categoria = new Categoria
            {
                Id = viewModel.Id,
                NombreCategoria = viewModel.NombreCategoria
            };
            await _cursoService.ActualizarCategoriaAsync(categoria);
            return Json(new { success = true, message = "Categoría actualizada con éxito." });
        }

        [HttpPost, ActionName("EliminarCategoria")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarCategoria(int id)
        {
            await _cursoService.EliminarCategoriaAsync(id);
            return RedirectToAction(nameof(GestionarCategorias));
        }

        // ----------------- Course Management Actions (CRUD) -----------------
        // GET: Gestionar Clases de un Curso Presencial
        [HttpGet]
        public async Task<IActionResult> GestionarClasesCurso(int cursoId)
        {
            var curso = await _cursoService.ObtenerCursoPorIdAsync(cursoId);
            if (curso == null)
            {
                return NotFound();
            }

            var clases = await _cursoService.ObtenerClasesPorCursoIdAsync(cursoId);

            var viewModel = new GestionClasesCursoViewModel
            {
                CursoId = cursoId,
                CursoTitulo = curso.Titulo,
                Clases = clases.Select(c => new ClaseConInscripcionesViewModel
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    FechaHoraInicio = c.FechaHoraInicio,
                    FechaHoraFin = c.FechaHoraFin,
                    CapacidadMaxima = c.CapacidadMaxima,
                    Lugar = c.Lugar,
                    Estado = c.Estado,
                    UrlMeet = c.UrlMeet,
                    InscritosCount = c.Inscripciones?.Count ?? 0,
                    Inscripciones = c.Inscripciones?.Select(i => new InscripcionClaseViewModel
                    {
                        Id = i.Id,
                        EstudianteId = i.EstudianteId,
                        EstudianteNombre = $"{i.Estudiante?.Nombre} {i.Estudiante?.Apellido}",
                        EstudianteEmail = i.Estudiante?.Email,
                        EstadoAsistencia = i.EstadoAsistencia,
                        FechaInscripcion = i.FechaInscripcion
                    }).ToList() ?? new List<InscripcionClaseViewModel>()
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Actualizar Asistencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarAsistencia([FromBody] ActualizarAsistenciaDto dto)
        {
            try
            {
                var result = await _cursoService.ActualizarAsistenciaAsync(dto.InscripcionId, dto.EstadoAsistencia);
                if (result)
                {
                    return Json(new { success = true, message = "Asistencia actualizada" });
                }
                return Json(new { success = false, message = "No se pudo actualizar la asistencia" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Editar Clase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarClase([FromBody] EditarClaseDto dto)
        {
            try
            {
                var clase = await _cursoService.ObtenerClasePorIdAsync(dto.Id);
                if (clase == null)
                {
                    return Json(new { success = false, message = "Clase no encontrada" });
                }

                clase.Titulo = dto.Titulo;
                clase.Descripcion = dto.Descripcion;
                clase.FechaHoraInicio = dto.FechaHoraInicio;
                clase.FechaHoraFin = dto.FechaHoraFin;
                clase.CapacidadMaxima = dto.CapacidadMaxima;
                clase.Lugar = dto.Lugar;
                clase.Estado = dto.Estado;
                clase.UrlMeet = dto.UrlMeet;

                await _cursoService.ActualizarClaseAsync(clase);
                return Json(new { success = true, message = "Clase actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> GestionarCursos()
        {
            var cursos = await _cursoService.ObtenerTodosLosCursosAsync();
            var viewModels = cursos.Select(c => new CursoViewModel
            {
                Id = c.Id,
                EsDestacado = c.EsDestacado,
                EsRecomendado = c.EsRecomendado,
                Titulo = c.Titulo,
                DescripcionCorta = c.DescripcionCorta,
                DescripcionLarga = c.DescripcionLarga,
                Precio = c.Precio,
                UrlImagen = c.UrlImagen,
                Nivel = c.Nivel,
                TipoCurso = c.TipoCurso,
                CategoriasSeleccionadas = c.CursoCategorias.Select(cc => cc.CategoriaId).ToArray(),
                InstructoresNombres = c.CursoInstructores
        .Select(ci => $"{ci.Artista.Usuario.Nombre} {ci.Artista.Usuario.Apellido}")
        .ToList()
            }).ToList();

            var categorias = await _cursoService.ObtenerTodasLasCategoriasAsync();
            var listaCategorias = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.NombreCategoria
            }).ToList();
            ViewBag.Niveles = new SelectList(Enum.GetValues(typeof(NivelDificultad)).Cast<NivelDificultad>().Select(v => new { Value = (int)v, Text = v.ToString() }), "Value", "Text");
            ViewBag.CategoriasCrear = listaCategorias;
            ViewBag.CategoriasEditar = listaCategorias;

            return View(viewModels);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerArtistas()
        {
            var artistas = await _artistaService.GetAllArtistasAsync();
            var artistasJson = artistas.Select(a => new {
                id = a.Id,
                nombreCompleto = $"{a.Usuario.Nombre} {a.Usuario.Apellido}"
            }).ToList();

            return Json(artistasJson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsociarArtistaACurso([FromBody] AsociarArtistaViewModel dto)
        {
            if (dto == null || dto.CursoId <= 0 || dto.InstructorId <= 0)
            {
                return Json(new { success = false, message = "Datos de solicitud inválidos." });
            }

            try
            {
                await _cursoService.AsociarArtistaACursoAsync(dto.CursoId, dto.InstructorId);
                return Json(new { success = true, message = "Artista asociado con éxito." });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error al asociar el artista." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerCursosJson()
        {
            var cursos = await _cursoService.ObtenerTodosLosCursosAsync();
            var viewModels = cursos.Select(c => new
            {
                id = c.Id,
                titulo = c.Titulo,
                descripcionCorta = c.DescripcionCorta,
                precio = c.Precio,
                urlImagen = c.UrlImagen,
                categoriasSeleccionadas = c.CursoCategorias.Select(cc => cc.CategoriaId).ToArray()
            }).ToList();
            return Json(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCursoPorIdJson(int id)
        {
            var curso = await _cursoService.ObtenerCursoPorIdAsync(id);

            if (curso == null)
            {
                return NotFound(new { success = false, message = "Curso no encontrado." });
            }

            var cursoViewModel = new CursoViewModel
            {
                Id = curso.Id,
                Titulo = curso.Titulo,
                DescripcionCorta = curso.DescripcionCorta,
                DescripcionLarga = curso.DescripcionLarga,
                Precio = curso.Precio,
                UrlImagen = curso.UrlImagen,
                CategoriasSeleccionadas = curso.CursoCategorias.Select(cc => cc.CategoriaId).ToArray()
            };

            return Json(new { success = true, data = cursoViewModel });
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerArtistasAsociados(int cursoId)
        {
            // Asume que tienes un método de servicio nuevo para esta operación
            var artistasAsociados = await _cursoService.ObtenerArtistasAsociadosACursoAsync(cursoId);

            var artistasJson = artistasAsociados.Select(a => new {
                id = a.Id,
                nombreCompleto = $"{a.NombreArtistico}"
            }).ToList();

            return Json(new { success = true, artistas = artistasJson });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesasociarArtistaACurso([FromBody] AsociarArtistaViewModel dto)
        {
            if (dto == null || dto.CursoId <= 0 || dto.InstructorId <= 0)
            {
                return Json(new { success = false, message = "Datos de solicitud inválidos." });
            }

            try
            {
                // Asume que tienes un método de servicio nuevo para esta operación
                await _cursoService.DesasociarArtistaACursoAsync(dto.CursoId, dto.InstructorId);
                return Json(new { success = true, message = "Artista desasociado con éxito." });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Ocurrió un error al desasociar el artista." });
            }
        }

        // Endpoints para gestionar clases presenciales
        [HttpGet]
        public async Task<IActionResult> ObtenerClasesPorCurso(int cursoId)
        {
            var clases = await _cursoService.ObtenerClasesPorCursoIdAsync(cursoId);
            var result = clases.Select(c => new {
                id = c.Id,
                titulo = c.Titulo,
                descripcion = c.Descripcion,
                fechaHoraInicio = c.FechaHoraInicio,
                fechaHoraFin = c.FechaHoraFin,
                capacidadMaxima = c.CapacidadMaxima,
                estado = c.Estado,
                inscritosCount = c.Inscripciones?.Count ?? 0
            });
            return Json(new { success = true, clases = result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearClase([FromBody] CrearClaseDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var clase = new ClasePresencial
                {
                    CursoId = dto.CursoId,
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    FechaHoraInicio = dto.FechaHoraInicio,
                    FechaHoraFin = dto.FechaHoraFin,
                    CapacidadMaxima = dto.CapacidadMaxima,
                    Lugar = dto.Lugar ?? "Estudio de la Academia",
                    Estado = "Programada",
                    UrlMeet = dto.UrlMeet,
                    NotasInstructor = "Te esperamos!"
                };

                await _cursoService.CrearClaseAsync(clase);
                return Json(new { success = true, message = "Clase creada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarClase(int id)
        {
            try
            {
                await _cursoService.EliminarClaseAsync(id);
                return Json(new { success = true, message = "Clase eliminada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CrearCurso([FromForm] CursoViewModel model)
        {
            IFormFile imagenArchivo = Request.Form.Files["imagenArchivo"];
            var categoriasSeleccionadas = Request.Form["CategoriasSeleccionadas"].Select(int.Parse).ToArray();

            // Validación básica
            if (string.IsNullOrEmpty(model.Titulo)) ModelState.AddModelError("Titulo", "El título es obligatorio.");
            if (imagenArchivo == null || imagenArchivo.Length == 0) ModelState.AddModelError("imagenArchivo", "La imagen es obligatoria.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                                       .ToDictionary(
                                           kvp => kvp.Key,
                                           kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                                       );
                return Json(new { success = false, errors = errors });
            }

            var sanitizer = new HtmlSanitizer();
            model.DescripcionCorta = sanitizer.Sanitize(model.DescripcionCorta);
            model.DescripcionLarga = sanitizer.Sanitize(model.DescripcionLarga);

            var curso = new Curso
            {
                Titulo = model.Titulo,
                DescripcionCorta = model.DescripcionCorta,
                DescripcionLarga = model.DescripcionLarga,
                Precio = model.Precio,
                UrlImagen = string.Empty,
                Nivel = model.Nivel,
                TipoCurso = model.TipoCurso  // NUEVO: asignar el tipo
            };

            // Lógica según el tipo de curso
            if (model.TipoCurso == TipoCurso.Digital)
            {
                await _cursoService.CrearCursoAsync(curso, categoriasSeleccionadas);
            }
            else
            {
                // NUEVO: Para cursos presenciales, las clases vienen del formulario
                var clases = new List<ClasePresencial>();

                // Procesar las clases del formulario (si vienen)
                var clasesTitulos = Request.Form["ClasesTitulo"].ToList();
                var clasesDescripciones = Request.Form["ClasesDescripcion"].ToList();
                var clasesFechasInicio = Request.Form["ClasesFechaHoraInicio"].ToList();
                var clasesFechasFin = Request.Form["ClasesFechaHoraFin"].ToList();
                var clasesCapacidades = Request.Form["ClasesCapacidadMaxima"].ToList();

                for (int i = 0; i < clasesTitulos.Count; i++)
                {
                    if (!string.IsNullOrEmpty(clasesTitulos[i]))
                    {
                        clases.Add(new ClasePresencial
                        {
                            Titulo = clasesTitulos[i],
                            Descripcion = clasesDescripciones.Count > i ? clasesDescripciones[i] : null,
                            FechaHoraInicio = DateTime.Parse(clasesFechasInicio[i]),
                            FechaHoraFin = DateTime.Parse(clasesFechasFin[i]),
                            CapacidadMaxima = clasesCapacidades.Count > i && int.TryParse(clasesCapacidades[i], out int cap) ? cap : 20,
                            Lugar = "Estudio de la Academia",
                            Estado = "Programada"
                        });
                    }
                }

                await _cursoService.CrearCursoPresencialAsync(curso, categoriasSeleccionadas, clases);
            }

            // Guardar imagen (mismo código que tenías)
            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cursos", curso.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var extension = Path.GetExtension(imagenArchivo.FileName);
                var fileName = $"imagen{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenArchivo.CopyToAsync(stream);
                }
                curso.UrlImagen = $"/cursos/{curso.Id}/{fileName}";
                await _cursoService.ActualizarCursoAsync(curso, categoriasSeleccionadas);
            }

            await _ventaItemService.CrearVentaItemAsync("Curso", curso.Id, null, curso.Precio);

            return Json(new { success = true, message = "Curso creado con éxito." });
        }
        [HttpPost]
        public async Task<IActionResult> EditarCurso(CursoViewModel model, IFormFile? imagenArchivo)
        {
            // 1. Validar los campos obligatorios del modelo
            // Se valida manualmente para evitar errores de model binding con la imagen y categorías
            if (string.IsNullOrEmpty(model.Titulo))
            {
                ModelState.AddModelError("Titulo", "El título es obligatorio.");
            }

            var sanitizer = new HtmlSanitizer();
            model.DescripcionCorta = sanitizer.Sanitize(model.DescripcionCorta);
            model.DescripcionLarga = sanitizer.Sanitize(model.DescripcionLarga);

            // Obtener las categorías seleccionadas del formulario de forma manual
            var categoriasSeleccionadas = Request.Form["CategoriasSeleccionadas"].Select(int.Parse).ToArray();

            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, devolver una respuesta JSON para que la maneje el frontend
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                                       .ToDictionary(
                                           kvp => kvp.Key,
                                           kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                                       );
                return Json(new { success = false, errors = errors });
            }

            // 2. Si la validación pasa, continuar con la lógica de edición
            var cursoExistente = await _cursoService.ObtenerCursoPorIdAsync(model.Id);
            if (cursoExistente == null)
            {
                // Devolver un error JSON si no se encuentra el curso
                return Json(new { success = false, message = "Curso no encontrado." });
            }

            cursoExistente.Titulo = model.Titulo;
            cursoExistente.DescripcionCorta = model.DescripcionCorta;
            cursoExistente.DescripcionLarga = model.DescripcionLarga;
            cursoExistente.Precio = model.Precio;
            cursoExistente.Nivel = model.Nivel;

            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
                // Lógica para guardar la nueva imagen y eliminar la antigua
                if (!string.IsNullOrEmpty(cursoExistente.UrlImagen))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cursoExistente.UrlImagen.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cursos", model.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var extension = Path.GetExtension(imagenArchivo.FileName);
                var fileName = $"imagen{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenArchivo.CopyToAsync(stream);
                }
                cursoExistente.UrlImagen = $"/cursos/{model.Id}/{fileName}";
            }

            await _cursoService.ActualizarCursoAsync(cursoExistente, categoriasSeleccionadas);

            var ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(cursoExistente.Id);
            if (ventaItem != null)
            {
                ventaItem.Precio = cursoExistente.Precio;
                await _ventaItemService.ActualizarVentaItemAsync(ventaItem);
            }

            // Devolver una respuesta JSON de éxito
            return Json(new { success = true, message = "Curso actualizado con éxito." });
        }

        [HttpPost, ActionName("EliminarCurso")]
        public async Task<IActionResult> ConfirmarEliminarCurso(int id)
        {
            await _cursoService.EliminarCursoAsync(id);
            return RedirectToAction(nameof(GestionarCursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DestacarCurso(int id)
        {
            var result = await _cursoService.DestacarCursoAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(GestionarCursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarDestacado(int id)
        {
            var result = await _cursoService.QuitarDestacadoAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(GestionarCursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecomendarCurso(int id)
        {
            var result = await _cursoService.RecomendarCursoAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(GestionarCursos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarRecomendado(int id)
        {
            var result = await _cursoService.QuitarRecomendadoAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(GestionarCursos));
        }

        // ----------------- Module Management Actions (CRUD) -----------------

        [HttpGet]
        public async Task<IActionResult> GestionarModulos(int cursoId)
        {
            var curso = await _cursoService.ObtenerCursoPorIdAsync(cursoId);
            if (curso == null)
            {
                return NotFound();
            }

            var modulos = await _moduloService.ObtenerModulosPorCursoIdAsync(cursoId);
            var modulosViewModels = modulos
                .OrderBy(m => m.Orden)
                .Select(m => new ModuloViewModel
                {
                    Id = m.Id,
                    CursoId = m.CursoId,
                    Titulo = m.Titulo,
                    Orden = m.Orden
                }).ToList();

            ViewBag.CursoId = cursoId;
            ViewBag.CursoTitulo = curso.Titulo;

            return View(modulosViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModulosPorCursoIdJson(int cursoId)
        {
            var modulos = await _moduloService.ObtenerModulosPorCursoIdAsync(cursoId);
            var modulosViewModels = modulos
                .OrderBy(m => m.Orden)
                .Select(m => new ModuloViewModel
                {
                    Id = m.Id,
                    CursoId = m.CursoId,
                    Titulo = m.Titulo,
                    Orden = m.Orden
                }).ToList();

            return Json(new { success = true, data = modulosViewModels });
        }

        [HttpPost]
        public async Task<IActionResult> CrearModulo([FromBody] ModuloViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }

            var modulo = new Modulo
            {
                CursoId = model.CursoId,
                Titulo = model.Titulo,
                Orden = model.Orden
            };

            var result = await _moduloService.CrearModuloAsync(modulo);

            if (result)
            {
                return Json(new { success = true, message = "Módulo creado con éxito." });
            }
            return Json(new { success = false, message = "No se pudo crear el módulo." });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModuloPorIdJson(int id)
        {
            var modulo = await _moduloService.ObtenerModuloPorIdAsync(id);
            if (modulo == null)
            {
                return NotFound(new { success = false, message = "Módulo no encontrado." });
            }

            var viewModel = new ModuloViewModel
            {
                Id = modulo.Id,
                CursoId = modulo.CursoId,
                Titulo = modulo.Titulo,
                Orden = modulo.Orden
            };

            return Json(new { success = true, data = viewModel });
        }

        [HttpPost]
        public async Task<IActionResult> EditarModulo([FromBody] ModuloViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }

            var modulo = await _moduloService.ObtenerModuloPorIdAsync(model.Id);
            if (modulo == null)
            {
                return NotFound(new { success = false, message = "Módulo no encontrado." });
            }

            modulo.Titulo = model.Titulo;
            modulo.Orden = model.Orden;

            var result = await _moduloService.ActualizarModuloAsync(modulo);

            if (result)
            {
                return Json(new { success = true, message = "Módulo actualizado con éxito." });
            }
            return Json(new { success = false, message = "No se pudo actualizar el módulo." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarModulo(int id)
        {
            var modulo = await _moduloService.ObtenerModuloPorIdAsync(id);
            if (modulo == null)
            {
                return NotFound();
            }
            var cursoId = modulo.CursoId;

            var result = await _moduloService.EliminarModuloAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Módulo eliminado con éxito.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar el módulo.";
            }
            return RedirectToAction("GestionarModulos", new { cursoId = cursoId });
        }

        // ----------------- Lesson Management Actions (CRUD) -----------------

        [HttpGet]
        public async Task<IActionResult> GestionarLecciones(int moduloId)
        {
            var modulo = await _moduloService.ObtenerModuloPorIdAsync(moduloId);
            if (modulo == null)
            {
                return NotFound();
            }

            var lecciones = await _leccionService.ObtenerLeccionesPorModuloIdAsync(moduloId);
            var leccionesViewModels = lecciones
                .OrderBy(l => l.Orden)
                .Select(l => new LeccionViewModel
                {
                    Id = l.Id,
                    ModuloId = l.ModuloId,
                    Titulo = l.Titulo,
                    Contenido = l.Contenido,
                    UrlVideo = l.UrlVideo,
                    Orden = l.Orden
                }).ToList();

            ViewBag.ModuloId = moduloId;
            ViewBag.ModuloTitulo = modulo.Titulo;
            ViewBag.CursoId = modulo.CursoId;

            return View(leccionesViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> CrearLeccion([FromBody] LeccionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }

            var sanitizer = new HtmlSanitizer();
            model.Contenido = sanitizer.Sanitize(model.Contenido); // ¡Nuevo! Saneamiento del HTML

            var leccion = new Leccion
            {
                ModuloId = model.ModuloId,
                Titulo = model.Titulo,
                Contenido = model.Contenido,
                UrlVideo = model.UrlVideo,
                Orden = model.Orden
            };

            var result = await _leccionService.CrearLeccionAsync(leccion);

            if (result)
            {
                return Json(new { success = true, message = "Lección creada con éxito." });
            }
            return Json(new { success = false, message = "No se pudo crear la lección." });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerLeccionPorIdJson(int id)
        {
            var leccion = await _leccionService.ObtenerLeccionPorIdAsync(id);
            if (leccion == null)
            {
                return NotFound(new { success = false, message = "Lección no encontrada." });
            }

            var viewModel = new LeccionViewModel
            {
                Id = leccion.Id,
                ModuloId = leccion.ModuloId,
                Titulo = leccion.Titulo,
                Contenido = leccion.Contenido,
                UrlVideo = leccion.UrlVideo,
                Orden = leccion.Orden
            };

            return Json(new { success = true, data = viewModel });
        }

        [HttpPost]
        public async Task<IActionResult> EditarLeccion([FromBody] LeccionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }

            var leccion = await _leccionService.ObtenerLeccionPorIdAsync(model.Id);
            if (leccion == null)
            {
                return NotFound(new { success = false, message = "Lección no encontrada." });
            }

            var sanitizer = new HtmlSanitizer();
            model.Contenido = sanitizer.Sanitize(model.Contenido); // ¡Nuevo! Saneamiento del HTML

            leccion.Titulo = model.Titulo;
            leccion.Contenido = model.Contenido;
            leccion.UrlVideo = model.UrlVideo;
            leccion.Orden = model.Orden;

            var result = await _leccionService.ActualizarLeccionAsync(leccion);

            if (result)
            {
                return Json(new { success = true, message = "Lección actualizada con éxito." });
            }
            return Json(new { success = false, message = "No se pudo actualizar la lección." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLeccion(int id)
        {
            var leccion = await _leccionService.ObtenerLeccionPorIdAsync(id);
            if (leccion == null)
            {
                return NotFound();
            }
            var moduloId = leccion.ModuloId;

            var result = await _leccionService.EliminarLeccionAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Lección eliminada con éxito.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar la lección.";
            }
            return RedirectToAction("GestionarLecciones", new { moduloId = moduloId });
        }
        private async Task<List<PedidoPendienteViewModel>> MapearPedidosAViewModelAsync(IEnumerable<Pedido> pedidos)
        {
            var viewModelList = new List<PedidoPendienteViewModel>();
            foreach (var pedido in pedidos)
            {
                var estudiante = await _userManager.FindByIdAsync(pedido.EstudianteId.ToString());
                // Nota: Asegúrese de que GetPagoWithPedido maneje si no existe el pago (e.g. Pedidos "Pendiente")
                var pago = await _pedidoService.GetPagoWithPedido(pedido.Id);

                viewModelList.Add(new PedidoPendienteViewModel
                {
                    PedidoId = pedido.Id,
                    NombreEstudiante = estudiante?.Nombre ?? "Estudiante Desconocido",
                    FechaPedido = pedido.FechaPedido,
                    Total = pedido.Total,
                    UrlComprobante = pago?.UrlComprobante,
                    Estado = pedido.Estado // Agregamos el estado
                });
            }
            return viewModelList;
        }
        public async Task<IActionResult> GestionarPedidos()
        {
            var dashboardModel = new DashboardPedidosViewModel();

            // 1. Pedidos en estado Validando (Requieren Aprobación/Anulación)
            var pedidosValidando = await _pedidoService.ObtenerPedidosPorEstadoAsync("Validando");
            dashboardModel.PedidosValidando = await MapearPedidosAViewModelAsync(pedidosValidando);

            // 2. Pedidos en estado Completado (Ya Aprobados)
            var pedidosCompletados = await _pedidoService.ObtenerPedidosPorEstadoAsync("Completado");
            dashboardModel.PedidosCompletados = await MapearPedidosAViewModelAsync(pedidosCompletados);

            // 3. Pedidos en estado Cancelado (Ya Anulados)
            var pedidosCancelados = await _pedidoService.ObtenerPedidosPorEstadoAsync("Cancelado");
            dashboardModel.PedidosCancelados = await MapearPedidosAViewModelAsync(pedidosCancelados);

            // 4. Pedidos en estado Pendiente (Pendientes de que el usuario suba comprobante)
            var pedidosPendientes = await _pedidoService.ObtenerPedidosPorEstadoAsync("Pendiente");
            dashboardModel.PedidosPendientes = await MapearPedidosAViewModelAsync(pedidosPendientes);


            return View("GestionarPedidos", dashboardModel);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> AprobarPedido(int pedidoId)
        {
            await _pedidoService.AprobarPedidoYOtorgarAccesoAsync(pedidoId);

            var pedido = await _pedidoService.ObtenerPedidoConItemsYVentaItemsAsync(pedidoId);
            if (pedido != null)
            {
                var usuario = await _userManager.FindByIdAsync(pedido.EstudianteId.ToString());
                var primerItem = pedido.PedidoItems.FirstOrDefault();

                if (primerItem != null)
                {
                    string tipoCompra = "";
                    string itemTitulo = "";
                    int cursoId = 0;

                    if (primerItem.VentaItem.Curso != null)
                    {
                        tipoCompra = "curso";
                        itemTitulo = primerItem.VentaItem.Curso.Titulo;
                        cursoId = primerItem.VentaItem.Curso.Id;

                        // Verificar si es un curso presencial
                        var curso = await _cursoService.ObtenerCursoPorIdAsync(cursoId);
                        if (curso != null && curso.TipoCurso == Core.Enums.TipoCurso.Presencial)
                        {
                            var clases = await _cursoService.ObtenerClasesPorCursoIdAsync(cursoId);
                            var claseProxima = clases
                                .Where(c => c.Estado == "Programada" && c.FechaHoraInicio > DateTime.Now)
                                .OrderBy(c => c.FechaHoraInicio)
                                .FirstOrDefault();

                            if (claseProxima != null)
                            {
                                var yaInscrito = await _cursoService.GetInscripcionByClaseAndEstudianteAsync(
                                    claseProxima.Id,
                                    pedido.EstudianteId);

                                if (yaInscrito == null)
                                {
                                    await _cursoService.InscribirEstudianteAClaseAsync(claseProxima.Id, pedido.EstudianteId);
                                }
                            }
                        }
                    }
                    else if (primerItem.VentaItem.Producto != null)
                    {
                        tipoCompra = "producto";
                        itemTitulo = primerItem.VentaItem.Producto.Titulo;
                    }
                    else if (primerItem.VentaItem.Paquete != null)
                    {
                        tipoCompra = "paquete";
                        itemTitulo = primerItem.VentaItem.Paquete.Titulo;
                    }

                    if (!string.IsNullOrEmpty(itemTitulo))
                    {
                        // ==================== ENVIAR CORREO AL ESTUDIANTE ====================
                        if (usuario != null)
                        {
                            try
                            {
                                await _emailService.EnviarCorreoCompraAprobadaAsync(
                                    usuario.Email,
                                    $"{usuario.Nombre} {usuario.Apellido}",
                                    pedidoId,
                                    tipoCompra,
                                    itemTitulo
                                );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al enviar correo de aprobación: {ex.Message}");
                            }
                        }

                        // Enviar notificación al usuario
                        await _notificacionService.CrearNotificacionPedidoAprobadoAsync(
                            pedido.EstudianteId,
                            pedidoId,
                            tipoCompra,
                            itemTitulo
                        );
                    }
                }
            }

            return RedirectToAction(nameof(GestionarPedidos));
        }
        [HttpPost]
        public async Task<IActionResult> AnularPedido(int pedidoId)
        {
            try
            {
                await _pedidoService.AnularPedidoAsync(pedidoId);

                TempData["MensajeAnulacion"] = $"El pedido #{pedidoId} ha sido anulado con éxito. ❌";
            }
            catch (KeyNotFoundException)
            {
                TempData["MensajeError"] = $"Error al anular: El pedido #{pedidoId} no fue encontrado.";
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error inesperado al intentar anular el pedido.";
            }

            return RedirectToAction(nameof(GestionarPedidos));
        }
        public async Task<IActionResult> GestionarAnuncios(int cursoId)
        {
            var anuncios = await _cursoService.ObtenerAnunciosPorCursoAsync(cursoId);
            // 5. Mapea a ViewModels
            var anunciosVIewModel = anuncios.Select(c => new AnuncioViewModel
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Contenido = c.Contenido,
                CursoId = c.CursoId,
                FechaPublicacion = c.FechaPublicacion,
            }).ToList();
            ViewBag.CursoId = cursoId; 
            return View(anunciosVIewModel);
        }

        // Acción para crear un nuevo anuncio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAnuncio(AnuncioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sanitizer = new HtmlSanitizer();
                model.Contenido = sanitizer.Sanitize(model.Contenido);

                var anuncio = new Anuncio()
                {
                    Titulo = model.Titulo,
                    Contenido = model.Contenido,
                    FechaPublicacion = DateTime.UtcNow,
                    CursoId = model.CursoId,
                };
                await _cursoService.CrearAnuncioAsync(anuncio);
                return RedirectToAction("GestionarAnuncios", new { cursoId = model.CursoId });
            }
            // Si hay errores de validación, se recarga la página
            return RedirectToAction("GestionarAnuncios", new { cursoId = model.CursoId });
        }

        // Acción para editar un anuncio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAnuncio(AnuncioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sanitizer = new HtmlSanitizer();
                model.Contenido = sanitizer.Sanitize(model.Contenido);

                var anuncio = new Anuncio()
                {
                    Id = model.Id,
                    Titulo = model.Titulo,
                    Contenido = model.Contenido,
                    FechaPublicacion = DateTime.UtcNow,
                    CursoId = model.CursoId,
                };
                await _cursoService.EditarAnuncioAsync(anuncio);
                return RedirectToAction("GestionarAnuncios", new { cursoId = model.CursoId });
            }
            // Si hay errores, se recarga la página
            return RedirectToAction("GestionarAnuncios", new { cursoId = model.CursoId });
        }

        // Acción para eliminar un anuncio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAnuncio(int id, int cursoId)
        {
            await _cursoService.EliminarAnuncioAsync(id);
            return RedirectToAction("GestionarAnuncios", new { cursoId = cursoId });
        }
        public async Task<IActionResult> GestionarEstudiantes(string searchTerm = "", string orden = "recientes", int page = 1)
        {
            const int pageSize = 15;

            var todosLosEstudiantes = await _userManager.GetUsersInRoleAsync("Estudiante");

            // Aplicar filtro de búsqueda
            var estudiantesFiltrados = todosLosEstudiantes.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                estudiantesFiltrados = estudiantesFiltrados.Where(e =>
                    (e.Nombre != null && e.Nombre.ToLower().Contains(searchTerm)) ||
                    (e.Apellido != null && e.Apellido.ToLower().Contains(searchTerm)) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchTerm)) ||
                    (e.Cedula != null && e.Cedula.Contains(searchTerm)));
            }

            // Aplicar orden
            switch (orden)
            {
                case "nombre-asc":
                    estudiantesFiltrados = estudiantesFiltrados.OrderBy(e => e.Nombre).ThenBy(e => e.Apellido);
                    break;
                case "nombre-desc":
                    estudiantesFiltrados = estudiantesFiltrados.OrderByDescending(e => e.Nombre).ThenByDescending(e => e.Apellido);
                    break;
                case "antiguos":
                    estudiantesFiltrados = estudiantesFiltrados.OrderBy(e => e.FechaCreacion);
                    break;
                case "recientes":
                default:
                    estudiantesFiltrados = estudiantesFiltrados.OrderByDescending(e => e.FechaCreacion);
                    break;
            }

            // Paginación
            var totalEstudiantes = estudiantesFiltrados.Count();
            var totalPaginas = (int)Math.Ceiling(totalEstudiantes / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPaginas && totalPaginas > 0) page = totalPaginas;

            var estudiantesPaginados = estudiantesFiltrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Obtener cursos comprados para cada estudiante (solo para mostrar en la tabla)
            var viewModel = new List<EstudianteListaViewModel>();

            foreach (var estudiante in estudiantesPaginados)
            {
                var pedidosAprobados = await _pedidoService.ObtenerPedidosAprobadosPorUsuarioAsync(estudiante.Id);
                var cursosComprados = new List<string>();

                foreach (var pedido in pedidosAprobados)
                {
                    var ventaItems = await _ventaItemService.ObtenerItemsPorPedidoIdAsync(pedido.Id);
                    foreach (var item in ventaItems)
                    {
                        if (item.Tipo == "Curso")
                        {
                            var curso = await _cursoService.ObtenerCursoPorIdAsync((int)item.CursoId);
                            if (curso != null)
                            {
                                cursosComprados.Add(curso.Titulo);
                            }
                        }
                    }
                }

                viewModel.Add(new EstudianteListaViewModel
                {
                    Id = estudiante.Id,
                    Nombre = estudiante.Nombre,
                    Apellido = estudiante.Apellido,
                    Email = estudiante.Email,
                    Cedula = estudiante.Cedula,
                    WhatsApp = estudiante.WhatsApp,
                    FechaRegistro = estudiante.FechaCreacion,
                    CursosCompradosCount = cursosComprados.Distinct().Count(),
                    CursosComprados = cursosComprados.Distinct().Take(3).ToList(),
                    TienePerfilCompleto = !string.IsNullOrEmpty(estudiante.Cedula) &&
                                          !string.IsNullOrEmpty(estudiante.WhatsApp) &&
                                          estudiante.FechaNacimiento.HasValue
                });
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Orden = orden;
            ViewBag.PaginaActual = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalEstudiantes = totalEstudiantes;

            return View(viewModel);
        }
        public async Task<IActionResult> GestionarArtistas()
        {
            var artistas = await _artistaService.GetAllArtistasAsync();

            // Proyectar la entidad a un ViewModel de lista
            var artistasViewModel = artistas.Select(a => new ArtistaListViewModel
            {
                Id = a.Id,
                UsuarioId = a.Usuario.Id,
                NombreCompleto = a.Usuario.Nombre + " " + a.Usuario.Apellido,
                Email = a.Usuario.Email,
                NombreArtistico = a.NombreArtistico,
                EstiloPrincipal = a.EstiloPrincipal,
                UrlImagen = a.Usuario.UrlImagen,
                Biografia = a.Biografia
            }).ToList();

            return View(artistasViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearArtista(ArtistaAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }
            string urlImagenDefecto = "/FotoPerfil/0/perfil_7cb6d6e2-17a9-4d14-91d1-5ce13752ef3b.jpg";

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Email = model.Email,
                UserName = model.Email,
                FechaCreacion = DateTime.UtcNow,
                UrlImagen = urlImagenDefecto,
            };

            var sanitizer = new Ganss.Xss.HtmlSanitizer(); // Asegúrate de que esta librería esté instalada
            model.Biografia = sanitizer.Sanitize(model.Biografia ?? string.Empty);

            var artista = new Artista
            {
                NombreArtistico = model.NombreArtistico,
                Biografia = model.Biografia,
                EstiloPrincipal = model.EstiloPrincipal
            };

            // 1. Primero, crea el artista y el usuario en la base de datos.
            // Esto es crucial, ya que asigna un ID válido al usuario.
            var (success, message) = await _artistaService.CrearArtistaAsync(usuario, model.Password, artista);

            if (!success)
            {
                // Si la creación falla, no hay nada más que hacer.
                return Json(new { success = false, message });
            }

            // 2. Si se subió una imagen, guárdala usando el ID del usuario recién creado.
            // Esta lógica va después de la creación.
            string? urlImagen = null;
            if (model.ImagenPerfil != null && model.ImagenPerfil.Length > 0)
            {
                urlImagen = await SaveImageAsync(model.ImagenPerfil, usuario.Id);
            }

            // 3. Actualiza la URL de la imagen en el registro del usuario.
            if (urlImagen != null)
            {
                usuario.UrlImagen = urlImagen;
                await _userManager.UpdateAsync(usuario);
            }

            return Json(new { success = true, message });
        }

        // POST: Actualiza un artista existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateArtista(ArtistaAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()) });
            }

            var artistaExistente = await _artistaService.GetByIdAsync(model.Id);
            if (artistaExistente == null)
            {
                return Json(new { success = false, message = "Artista no encontrado." });
            }

            var usuarioExistente = await _userManager.FindByIdAsync(model.UsuarioId.ToString());
            if (usuarioExistente == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            usuarioExistente.Nombre = model.Nombre;
            usuarioExistente.Apellido = model.Apellido;
            usuarioExistente.Email = model.Email;
            usuarioExistente.UserName = model.Email;

            artistaExistente.NombreArtistico = model.NombreArtistico;
            var sanitizer = new Ganss.Xss.HtmlSanitizer(); // Asegúrate de que esta librería esté instalada
            model.Biografia = sanitizer.Sanitize(model.Biografia ?? string.Empty);
            artistaExistente.Biografia = model.Biografia;
            artistaExistente.EstiloPrincipal = model.EstiloPrincipal;

            string? nuevaUrlImagen = null;
            if (model.ImagenPerfil != null && model.ImagenPerfil.Length > 0)
            {
                // Eliminar imagen anterior si existe
                if (!string.IsNullOrEmpty(usuarioExistente.UrlImagen))
                {
                    DeleteImage(usuarioExistente.UrlImagen);
                }
                // Guardar nueva imagen
                nuevaUrlImagen = await SaveImageAsync(model.ImagenPerfil, usuarioExistente.Id);
            }

            var (success, message) = await _artistaService.UpdateArtistaAsync(usuarioExistente, model.Password, artistaExistente, nuevaUrlImagen);

            if (success)
            {
                return Json(new { success = true, message });
            }

            return Json(new { success = false, message });
        }

        // POST: Elimina un artista
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArtista(int id)
        {
            var artista = await _artistaService.GetByIdAsync(id);
            if (artista == null)
            {
                return Json(new { success = false, message = "Artista no encontrado." });
            }

            // El controlador elimina el archivo del sistema de archivos
            if (!string.IsNullOrEmpty(artista.Usuario?.UrlImagen))
            {
                DeleteImage(artista.Usuario.UrlImagen);
            }

            // El controlador llama al servicio para eliminar las entidades de la base de datos
            var (success, message) = await _artistaService.DeleteArtistaAsync(id);

            if (success)
            {
                return Json(new { success = true, message });
            }

            return Json(new { success = false, message });
        }

        // Métodos auxiliares para la gestión de archivos (pueden ser movidos a una clase auxiliar)
        private async Task<string> SaveImageAsync(IFormFile file, int userId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FotoPerfil", userId.ToString());
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"perfil_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/FotoPerfil/{userId}/{fileName}";
        }

        private void DeleteImage(string urlImagen)
        {
            if (string.IsNullOrEmpty(urlImagen)) return;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", urlImagen.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GestionarEventos()
        {
            var eventos = await _eventoService.GetEventosAsync();

            var viewModel = eventos.Select(e => new EventoViewModel
            {
                Id = e.Id,
                Titulo = e.Titulo,
                FechaEvento = e.FechaEvento,
                Lugar = e.Lugar,
                TipoEvento = e.TipoEvento?.NombreTipo, // Usamos la propiedad de navegación
                Plataforma = e.TipoEvento?.Plataforma, // Usamos la propiedad de navegación
                EstaActivo = e.EstaActivo,
                UrlImagen = e.UrlImagen
            }).ToList();

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CrearEvento()
        {
            var tiposEvento = await _eventoService.GetTiposEventosAsync();
            var viewModel = new CrearEventoViewModel
            {
                FechaEvento = DateTime.Now,
                TiposEventoDisponibles = tiposEvento.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.NombreTipo} ({t.Plataforma})"
                })
            };
            return View(viewModel);
        }

        // Método POST para procesar el formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEvento(CrearEventoViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.Descripcion))
            {
                var sanitizer = new HtmlSanitizer();
                viewModel.Descripcion = sanitizer.Sanitize(viewModel.Descripcion);
            }
            if (ModelState.IsValid)
            {
                // Lógica para subir la imagen al servidor
                string rutaImagen = string.Empty;
                if (viewModel.ImagenPortada != null)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "eventos/portadas");
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImagenPortada.FileName);
                    string filePath = Path.Combine(uploadsFolder, nombreArchivo);

                    // Asegura que el directorio exista
                    Directory.CreateDirectory(uploadsFolder);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImagenPortada.CopyToAsync(fileStream);
                    }
                    rutaImagen = "/eventos/portadas/" + nombreArchivo;
                }

                // Mapear el ViewModel a la entidad de dominio
                var nuevoEvento = new Evento
                {
                    Titulo = viewModel.Titulo,
                    Descripcion = viewModel.Descripcion,
                    FechaEvento = viewModel.FechaEvento,
                    Lugar = viewModel.Lugar,
                    UrlImagen = rutaImagen,
                    EsDestacado = viewModel.EsDestacado,
                    EstaActivo = true,
                    FechaCreacion = DateTime.Now,
                    TipoEventoId = viewModel.TipoEventoId
                };

                await _eventoService.AddEventoAsync(nuevoEvento);
                TempData["MensajeExito"] = "Evento creado exitosamente.";
                return RedirectToAction(nameof(GestionarEventos));
            }

            // Si el modelo no es válido, vuelve a cargar la lista de tipos de evento
            var tiposEvento = await _eventoService.GetTiposEventosAsync();
            viewModel.TiposEventoDisponibles = tiposEvento.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.NombreTipo} ({t.Plataforma})"
            });

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> EditarEvento(int id)
        {
            var evento = await _eventoService.GetEventoByIdAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            var tiposEvento = await _eventoService.GetTiposEventosAsync();

            var viewModel = new CrearEventoViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Descripcion = evento.Descripcion,
                FechaEvento = evento.FechaEvento,
                Lugar = evento.Lugar,
                EsDestacado = evento.EsDestacado,
                TipoEventoId = evento.TipoEventoId,
                TiposEventoDisponibles = tiposEvento.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.NombreTipo} ({t.Plataforma})"
                }),
                UrlImagenExistente = evento.UrlImagen,
            };

            // Guardamos la URL de la imagen existente en TempData para mostrarla en la vista
            TempData["UrlImagenExistente"] = evento.UrlImagen;

            return View(viewModel);
        }

        // Método POST para procesar el formulario de edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEvento(int id, CrearEventoViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(viewModel.Descripcion))
            {
                var sanitizer = new HtmlSanitizer();
                viewModel.Descripcion = sanitizer.Sanitize(viewModel.Descripcion);
            }
            if (ModelState.IsValid)
            {
                var eventoAActualizar = await _eventoService.GetEventoByIdAsync(id);
                if (eventoAActualizar == null)
                {
                    return NotFound();
                }

                // Lógica para subir una nueva imagen si se proporciona
                if (viewModel.ImagenPortada != null)
                {
                    // Eliminar la imagen antigua si existe
                    if (!string.IsNullOrEmpty(eventoAActualizar.UrlImagen))
                    {
                        var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, eventoAActualizar.UrlImagen.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Guardar la nueva imagen
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "eventos/portadas");
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImagenPortada.FileName);
                    string filePath = Path.Combine(uploadsFolder, nombreArchivo);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ImagenPortada.CopyToAsync(fileStream);
                    }
                    eventoAActualizar.UrlImagen = "/eventos/portadas/" + nombreArchivo;
                }

                // Mapear los datos del ViewModel a la entidad de dominio
                eventoAActualizar.Titulo = viewModel.Titulo;
                eventoAActualizar.Descripcion = viewModel.Descripcion;
                eventoAActualizar.FechaEvento = viewModel.FechaEvento;
                eventoAActualizar.Lugar = viewModel.Lugar;
                eventoAActualizar.EsDestacado = viewModel.EsDestacado;
                eventoAActualizar.TipoEventoId = viewModel.TipoEventoId;

                await _eventoService.UpdateEventoAsync(eventoAActualizar);

                TempData["MensajeExito"] = "Evento actualizado exitosamente.";
                return RedirectToAction(nameof(GestionarEventos));
            }

            // Si el modelo no es válido, vuelve a cargar la lista de tipos de evento
            var tiposEvento = await _eventoService.GetTiposEventosAsync();
            viewModel.TiposEventoDisponibles = tiposEvento.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.NombreTipo} ({t.Plataforma})"
            });

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEventoDirecto(int id)
        {
            var eventoAEliminar = await _eventoService.GetEventoByIdAsync(id);

            if (eventoAEliminar == null)
            {
                return NotFound();
            }

            // Lógica para eliminar la imagen física del servidor
            if (!string.IsNullOrEmpty(eventoAEliminar.UrlImagen))
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, eventoAEliminar.UrlImagen.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _eventoService.DeleteEventoAsync(id);

            TempData["MensajeExito"] = "Evento eliminado exitosamente.";
            return RedirectToAction(nameof(GestionarEventos));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirFoto(SubirEventoFotosViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Lógica para guardar la imagen en el servidor
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "eventos/galeria");
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.Foto.FileName);
                string filePath = Path.Combine(uploadsFolder, nombreArchivo);

                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.Foto.CopyToAsync(fileStream);
                }

                var nuevaFoto = new EventoFotos
                {
                    UrlImagen = "/eventos/galeria/" + nombreArchivo,
                    FechaSubida = DateTime.Now,
                    EventoId = viewModel.EventoId
                };

                await _eventoService.AddEventoFotoAsync(nuevaFoto); // Necesitas implementar este método en tu servicio y repositorio

                TempData["MensajeExito"] = "Foto subida exitosamente.";
            }

            return RedirectToAction(nameof(GestionarEventos));
        }
        [HttpGet]
        public async Task<IActionResult> GestionarProductos()
        {
            var productos = await _productoService.GetAllAsync();
            var autores = await _productoService.GetAutoresAsync();
            var categorias = await _productoService.GetCategoriasAsync();

            var productosViewModel = productos.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Titulo = p.Titulo,
                TipoProducto = p.TipoProducto,
                Precio = p.Precio,
                AutorNombre = p.Autor?.NombreArtistico,
                Categorias = string.Join(", ", p.ProductoCategorias.Select(pc => pc.Categoria.NombreCategoria)),
                UrlImagen = p.UrlImagen,
            }).ToList();

            var autoresDisponibles = autores.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.NombreArtistico
            });

            var categoriasDisponibles = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.NombreCategoria
            });

            var viewModel = new GestionarProductosViewModel
            {
                Productos = productosViewModel,
                NuevoProducto = new CrearProductoViewModel(), // Instancia para el formulario del modal
                AutoresDisponibles = autoresDisponibles,
                CategoriasDisponibles = categoriasDisponibles
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto([Bind(Prefix = "NuevoProducto")] CrearProductoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string urlImagen = null;
                if (viewModel.ImagenPortada != null)
                {
                    urlImagen = await GuardarArchivo(viewModel.ImagenPortada, "productos/portadas");
                }
                var sanitizer = new HtmlSanitizer();
                viewModel.DescripcionLarga = sanitizer.Sanitize(viewModel.DescripcionLarga);
                viewModel.DescripcionCorta = sanitizer.Sanitize(viewModel.DescripcionCorta);


                string urlArchivoDigital = null;
                if (viewModel.ArchivoDigital != null)
                {
                    urlArchivoDigital = await GuardarArchivo(viewModel.ArchivoDigital, "productos/archivos");
                }

                var nuevoProducto = new Producto
                {
                    Titulo = viewModel.Titulo,
                    DescripcionCorta = viewModel.DescripcionCorta,
                    DescripcionLarga = viewModel.DescripcionLarga,
                    TipoProducto = viewModel.TipoProducto,
                    Precio = viewModel.Precio,
                    UrlImagen = urlImagen,
                    UrlArchivoDigital = urlArchivoDigital,
                    FechaPublicacion = DateTime.UtcNow,
                    AutorId = viewModel.AutorId
                };

                await _productoService.AddProductoAsync(nuevoProducto, viewModel.CategoriaIds);

                TempData["MensajeExito"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(GestionarProductos));
            }

            await CargarDatosViewModel(viewModel);

            return RedirectToAction(nameof(GestionarProductos));
        }

        // Métodos auxiliares para la lógica de archivos y la carga de datos del ViewModel
        private async Task<string> GuardarArchivo(IFormFile archivo, string carpetaDestino)
        {
            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, carpetaDestino);
            string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string filePath = Path.Combine(uploadsFolder, nombreArchivo);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            return $"/{carpetaDestino}/{nombreArchivo}";
        }

        private async Task CargarDatosViewModel(CrearProductoViewModel viewModel)
        {
            viewModel.TiposProductoDisponibles = new List<SelectListItem>
    {
        new SelectListItem { Value = "pdf", Text = "PDF" },
        new SelectListItem { Value = "revista", Text = "Revista" }
    };

            var autores = await _productoService.GetAutoresAsync();
            viewModel.AutoresDisponibles = autores.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.NombreArtistico
            });

            var categorias = await _productoService.GetCategoriasAsync();
            viewModel.CategoriasDisponibles = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.NombreCategoria
            });
        }
        [HttpGet]
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            var viewModel = new CrearProductoViewModel
            {
                Id = producto.Id,
                Titulo = producto.Titulo,
                DescripcionCorta = producto.DescripcionCorta,
                DescripcionLarga = producto.DescripcionLarga,
                TipoProducto = producto.TipoProducto,
                Precio = producto.Precio,
                AutorId = producto.AutorId,
                CategoriaIds = producto.ProductoCategorias.Select(pc => pc.CategoriaId).ToList()
            };

            await CargarDatosViewModel(viewModel);

            // Guardamos las URLs existentes para mostrarlas en la vista y para la lógica de eliminación
            TempData["UrlImagenExistente"] = producto.UrlImagen;
            TempData["UrlArchivoExistente"] = producto.UrlArchivoDigital;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(int id, CrearProductoViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var productoAActualizar = await _productoService.GetByIdAsync(id);
                if (productoAActualizar == null)
                {
                    return NotFound();
                }

                var sanitizer = new HtmlSanitizer();
                viewModel.DescripcionLarga = sanitizer.Sanitize(viewModel.DescripcionLarga);
                viewModel.DescripcionCorta = sanitizer.Sanitize(viewModel.DescripcionCorta);

                // Lógica para actualizar la imagen de portada
                if (viewModel.ImagenPortada != null)
                {
                    EliminarArchivo(productoAActualizar.UrlImagen); // Elimina el archivo antiguo
                    productoAActualizar.UrlImagen = await GuardarArchivo(viewModel.ImagenPortada, "productos/portadas");
                }

                // Lógica para actualizar el archivo digital
                if (viewModel.ArchivoDigital != null)
                {
                    EliminarArchivo(productoAActualizar.UrlArchivoDigital); // Elimina el archivo antiguo
                    productoAActualizar.UrlArchivoDigital = await GuardarArchivo(viewModel.ArchivoDigital, "productos/archivos");
                }

                // Actualizar las propiedades del producto
                productoAActualizar.Titulo = viewModel.Titulo;
                productoAActualizar.DescripcionCorta = viewModel.DescripcionCorta;
                productoAActualizar.DescripcionLarga = viewModel.DescripcionLarga;
                productoAActualizar.TipoProducto = viewModel.TipoProducto;
                productoAActualizar.Precio = viewModel.Precio;
                productoAActualizar.AutorId = viewModel.AutorId;

                await _productoService.UpdateProductoAsync(productoAActualizar, viewModel.CategoriaIds);

                TempData["MensajeExito"] = "Producto actualizado exitosamente.";
                return RedirectToAction(nameof(GestionarProductos));
            }

            // Si la validación falla, recargamos las listas para la vista
            await CargarDatosViewModel(viewModel);
            return View(viewModel);
        }
        private void EliminarArchivo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var path = Path.Combine(_hostingEnvironment.WebRootPath, url.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var productoAEliminar = await _productoService.GetByIdAsync(id);
            if (productoAEliminar == null)
            {
                return NotFound();
            }

            // Elimina el archivo de imagen física si existe
            if (!string.IsNullOrEmpty(productoAEliminar.UrlImagen))
            {
                EliminarArchivo(productoAEliminar.UrlImagen);
            }

            // Elimina el archivo digital (PDF) si existe
            if (!string.IsNullOrEmpty(productoAEliminar.UrlArchivoDigital))
            {
                EliminarArchivo(productoAEliminar.UrlArchivoDigital);
            }

            await _productoService.DeleteProductoAsync(id);

            TempData["MensajeExito"] = "Producto eliminado exitosamente.";
            return RedirectToAction(nameof(GestionarProductos));
        }

        [HttpGet]
        public async Task<IActionResult> GestionarRecursosActividades(string tipoEntidad, int entidadId)
        {
            if (string.IsNullOrEmpty(tipoEntidad) || entidadId <= 0)
            {
                return BadRequest("Tipo de entidad o ID inválido.");
            }

            // 1. Obtener la información de la entidad para el contexto de la vista (Título)
            string entidadNombre = string.Empty;
            int? cursoId = null;

            if (tipoEntidad.Equals("Curso", StringComparison.OrdinalIgnoreCase))
            {
                var curso = await _cursoService.ObtenerCursoPorIdAsync(entidadId);
                entidadNombre = curso?.Titulo ?? "Curso Desconocido";
                cursoId = entidadId;
            }
            else if (tipoEntidad.Equals("Modulo", StringComparison.OrdinalIgnoreCase))
            {
                var modulo = await _moduloService.ObtenerModuloPorIdAsync(entidadId);
                entidadNombre = modulo?.Titulo ?? "Módulo Desconocido";
                cursoId = modulo?.CursoId;
            }
            else if (tipoEntidad.Equals("Leccion", StringComparison.OrdinalIgnoreCase))
            {
                var leccion = await _leccionService.ObtenerLeccionPorIdAsync(entidadId);
                entidadNombre = leccion?.Titulo ?? "Lección Desconocida";
                var modulo = await _moduloService.ObtenerModuloPorIdAsync(leccion.ModuloId);
                cursoId = modulo?.CursoId;
            }

            if (string.IsNullOrEmpty(entidadNombre) || cursoId == null)
            {
                return NotFound($"No se encontró la entidad de tipo {tipoEntidad} con ID {entidadId}.");
            }

            // 2. Obtener los recursos/actividades existentes
            var recursos = await _recursoActividadService.ObtenerRecursosDeEntidadAsync(tipoEntidad, entidadId);

            // 3. Preparar ViewBags para el formulario de creación
            ViewBag.TipoEntidad = tipoEntidad;
            ViewBag.EntidadId = entidadId;
            ViewBag.EntidadNombre = entidadNombre;
            ViewBag.CursoId = cursoId;

            // Opciones para TipoRecurso
            ViewBag.TiposRecurso = new SelectList(new List<string> { "Actividad", "PDF", "Enlace", "ArchivoDescargable" });

            return View(recursos.Select(r => new RecursoActividadViewModel
            {
                Id = r.Id,
                Titulo = r.Titulo,
                TipoRecurso = r.TipoRecurso,
                Descripcion = r.Descripcion,
                Url = r.Url,
                RequiereEntrega = r.RequiereEntrega,
                EntidadNombre = entidadNombre // Para la lista
            }).ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRecursoActividad([FromForm] RecursoActividadViewModel model)
        {
            // --- 1. Validación de Archivo/URL específica ---
            // Si no es una Actividad y no tiene URL ni Archivo, agregamos un error.
            if (string.IsNullOrEmpty(model.Url) && (model.Archivo == null || model.Archivo.Length == 0))
            {
                ModelState.AddModelError("Url", "Debe proporcionar una URL o subir un archivo para este recurso.");
            }

            // Validación adicional para actividades: si tiene URL, debe ser válida
            if (model.TipoRecurso == "Actividad" && !string.IsNullOrEmpty(model.Url))
            {
                // Si es una URL de YouTube, extraer el ID para validación básica
                if (model.Url.Contains("youtube.com") || model.Url.Contains("youtu.be"))
                {
                    var videoIdMatch = System.Text.RegularExpressions.Regex.Match(model.Url,
                        @"(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|shorts\/))([\w-]{11})");

                    if (!videoIdMatch.Success)
                    {
                        ModelState.AddModelError("Url", "La URL de YouTube no es válida. Debe ser un enlace de video estándar.");
                    }
                }
            }

            // --- 2. Manejo de Errores de Validación del Modelo ---
            if (!ModelState.IsValid)
            {
                // Mapea todos los errores de validación a un diccionario para devolverlos en JSON
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                // Devuelve una respuesta JSON indicando el fallo y los errores.
                return Json(new
                {
                    success = false,
                    errors = errors,
                    message = "Error de validación al crear el recurso. Revise los campos."
                });
            }

            // --- 3. Saneamiento de datos y Preparación de URL ---
            var sanitizer = new Ganss.Xss.HtmlSanitizer(); // Asegúrate de que esta librería esté instalada
            model.Descripcion = sanitizer.Sanitize(model.Descripcion ?? string.Empty);

            string finalUrl = model.Url;

            // --- 4. Manejo de Subida de Archivos (si aplica) ---
            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                try
                {
                    // Define la ruta de subida: wwwroot/recursos/{TipoEntidad}/{EntidadId}/
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "recursos", model.TipoEntidad, model.EntidadId.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var extension = Path.GetExtension(model.Archivo.FileName);
                    // Usar un GUID para garantizar un nombre de archivo único
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Archivo.CopyToAsync(stream);
                    }

                    // La URL final que se guarda en la DB es relativa a wwwroot
                    finalUrl = $"/recursos/{model.TipoEntidad}/{model.EntidadId}/{fileName}";
                }
                catch (Exception ex)
                {
                    // Error durante la subida del archivo al disco
                    return Json(new
                    {
                        success = false,
                        message = $"Error al subir el archivo: {ex.Message}"
                    });
                }
            }

            // --- 5. Mapeo a entidad y Creación ---
            var recursoActividad = new RecursoActividad
            {
                TipoEntidad = model.TipoEntidad,
                EntidadId = model.EntidadId,
                Titulo = model.Titulo,
                Descripcion = model.Descripcion ?? string.Empty,
                TipoRecurso = model.TipoRecurso,
                Url = finalUrl,
                RequiereEntrega = model.RequiereEntrega,
                FechaCreacion = DateTime.UtcNow
            };

            var resultado = await _recursoActividadService.CrearRecursoActividadAsync(recursoActividad);

            if (resultado != null)
            {
                // Éxito: Devuelve JSON. El JavaScript se encargará de mostrar SweetAlert.
                return Json(new
                {
                    success = true,
                    message = $"Recurso/Actividad '{recursoActividad.Titulo}' creado con éxito."
                });
            }
            else
            {
                // Error en el servicio o la base de datos: Devuelve JSON.
                return Json(new
                {
                    success = false,
                    message = "Error al crear el recurso/actividad en la base de datos."
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerRecursoParaEdicion(int id)
        {
            var recurso = await _recursoActividadService.ObtenerRecursoPorIdAsync(id);

            if (recurso == null)
            {
                return NotFound(new { message = "Recurso no encontrado." });
            }

            // Mapeo a ViewModel para la edición
            var viewModel = new RecursoActividadViewModel
            {
                Id = recurso.Id,
                TipoEntidad = recurso.TipoEntidad,
                EntidadId = recurso.EntidadId,
                Titulo = recurso.Titulo,
                Descripcion = recurso.Descripcion,
                TipoRecurso = recurso.TipoRecurso,
                Url = recurso.Url,
                RequiereEntrega = recurso.RequiereEntrega
                // No se mapea Archivo aquí.
            };

            // Devuelve el JSON que el JavaScript usará para llenar el modal de edición
            return Json(new { success = true, data = viewModel });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecursoActividad([FromForm] RecursoActividadViewModel model)
        {
            // 1. Obtención y Validación del ID
            if (model.Id <= 0)
            {
                return Json(new { success = false, message = "ID de recurso inválido para edición." });
            }

            // --- 2. Validación de Archivo/URL específica ---
            // Si no es una Actividad y no tiene URL ni Archivo, agregamos un error.
            // Esto es crítico si el usuario borra la URL existente y no sube un archivo nuevo.
            if (string.IsNullOrEmpty(model.Url) && (model.Archivo == null || model.Archivo.Length == 0))
            {
                ModelState.AddModelError("Url", "Debe proporcionar una URL o subir/reemplazar un archivo para este recurso.");
            }

            // Validación adicional para actividades
            if (model.TipoRecurso == "Actividad" && !string.IsNullOrEmpty(model.Url))
            {
                if (model.Url.Contains("youtube.com") || model.Url.Contains("youtu.be"))
                {
                    var videoIdMatch = System.Text.RegularExpressions.Regex.Match(model.Url,
                        @"(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|shorts\/))([\w-]{11})");

                    if (!videoIdMatch.Success)
                    {
                        ModelState.AddModelError("Url", "La URL de YouTube no es válida. Debe ser un enlace de video estándar.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return Json(new { success = false, errors = errors, message = "Error de validación al actualizar el recurso. Revise los campos." });
            }

            // 3. Obtener el recurso existente para ver el archivo/URL anterior
            var recursoExistente = await _recursoActividadService.ObtenerRecursoPorIdAsync(model.Id);
            if (recursoExistente == null)
            {
                return Json(new { success = false, message = "El recurso a editar no existe." });
            }

            // **NOTA:** Aquí debes obtener el instructorId del usuario logueado.
            // Usaremos el valor existente en tu código para seguir la estructura.
            int instructorId = 1;

            // 4. Manejo de Archivo: Reemplazo o Conservación de URL
            string finalUrl = model.Url;

            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                // a. ELIMINAR archivo anterior (si existe y no es una URL externa)
                if (!string.IsNullOrEmpty(recursoExistente.Url) && !recursoExistente.Url.StartsWith("http"))
                {
                    var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, recursoExistente.Url.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // b. SUBIR nuevo archivo (reutilizando la lógica de CrearRecursoActividad)
                try
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "recursos", model.TipoEntidad, model.EntidadId.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var extension = Path.GetExtension(model.Archivo.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Archivo.CopyToAsync(stream);
                    }

                    // La URL final que se guarda es la del nuevo archivo
                    finalUrl = $"/recursos/{model.TipoEntidad}/{model.EntidadId}/{fileName}";
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error al subir el nuevo archivo: {ex.Message}" });
                }
            }
            // Si no se sube un nuevo archivo, finalUrl conserva el valor del formulario (model.Url),
            // que puede ser el URL anterior o vacío. Esto es correcto si no era un archivo físico.

            // 5. Saneamiento de datos
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            model.Descripcion = sanitizer.Sanitize(model.Descripcion ?? string.Empty);

            // 6. Mapeo a entidad
            var recursoActividad = new RecursoActividad
            {
                Id = model.Id,
                // Usamos los valores del modelo, o el existente si son nulos (aunque los hidden fields deberían enviarlos)
                TipoEntidad = model.TipoEntidad,
                EntidadId = model.EntidadId,
                Titulo = model.Titulo,
                Descripcion = model.Descripcion,
                TipoRecurso = model.TipoRecurso,
                Url = finalUrl, // <-- Usamos la URL actualizada
                RequiereEntrega = model.RequiereEntrega
            };

            var resultado = await _recursoActividadService.ActualizarRecursoActividadAsync(recursoActividad, model.Id, instructorId);

            if (resultado)
            {
                return Json(new { success = true, message = $"Recurso/Actividad '{model.Titulo}' actualizado con éxito." });
            }
            else
            {
                return Json(new { success = false, message = "Error al actualizar el recurso/actividad. Verifique permisos o si el ID existe." });
            }
        }

        // =================================
        // 3.3. ENDPOINT PARA ELIMINACIÓN (POST)
        // =================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRecursoActividad(int id)
        {
            // **NOTA:** Aquí debes obtener el instructorId del usuario logueado.
            // int instructorId = ObtenerInstructorIdLogueado(); 
            int instructorId = 1; // AJUSTAR ESTO

            if (id <= 0)
            {
                return Json(new { success = false, message = "ID de recurso inválido para eliminación." });
            }

            var recurso = await _recursoActividadService.ObtenerRecursoPorIdAsync(id);
            if (recurso == null)
            {
                return Json(new { success = false, message = "El recurso no existe." });
            }

            // Opcional: Lógica para ELIMINAR el archivo físico (si existe)
            if (!string.IsNullOrEmpty(recurso.Url) && !recurso.Url.StartsWith("http"))
            {
                Path.Combine(_hostingEnvironment.WebRootPath, recurso.Url);
            }

            var resultado = await _recursoActividadService.EliminarRecursoActividadAsync(id, instructorId);

            if (resultado)
            {
                return Json(new { success = true, message = $"Recurso '{recurso.Titulo}' eliminado con éxito." });
            }
            else
            {
                return Json(new { success = false, message = "Error al eliminar el recurso. Verifique permisos o restricciones de la base de datos." });
            }
        }
        public async Task<IActionResult> Articulos()
        {
            var articulos = await _articuloService.GetAllArticulosAsync();
            var viewModels = articulos.Select(a => new ArticuloViewModel
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Categoria = a.Categoria,
                Estado = a.Estado,
                NombrePersonaDestacada = a.NombrePersonaDestacada
                // Solo mapeamos las propiedades necesarias para la tabla
            }).ToList();
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult CrearArticulo()
        {
            var viewModel = new ArticuloViewModel
            {
                Estado = "Borrador"
            };
            return View("CrearEditarArticulo", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearArticulo(ArticuloViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CrearEditarArticulo", model);
            }

            var sanitizer = new HtmlSanitizer();
            model.ContenidoTexto = sanitizer.Sanitize(model.ContenidoTexto);
            model.BiografiaCortaDestacado = sanitizer.Sanitize(model.BiografiaCortaDestacado);

            var articulo = new Articulo
            {
                Titulo = model.Titulo,
                Resumen = model.Resumen,
                ContenidoTexto = model.ContenidoTexto,
                Categoria = model.Categoria,
                Estado = model.Estado,
                NombrePersonaDestacada = model.NombrePersonaDestacada,
                BiografiaCortaDestacado = model.BiografiaCortaDestacado,
            };

            await _articuloService.CreateArticuloAsync(
                articulo,
                model.FotoDestacadoFile,
                model.ImagenPrincipalFile,
                model.FotosContenidoFiles?.ToList());

            TempData["SuccessMessage"] = "Artículo creado exitosamente.";
            return RedirectToAction(nameof(Articulos));
        }

        [HttpGet]
        public async Task<IActionResult> EditarArticulo(int id)
        {
            var articulo = await _articuloService.GetArticuloByIdAsync(id);
            if (articulo == null)
            {
                return NotFound();
            }

            var viewModel = new ArticuloViewModel
            {
                Id = articulo.Id,
                Titulo = articulo.Titulo,
                Resumen = articulo.Resumen,
                ContenidoTexto = articulo.ContenidoTexto,
                Categoria = articulo.Categoria,
                Estado = articulo.Estado,
                NombrePersonaDestacada = articulo.NombrePersonaDestacada,
                BiografiaCortaDestacado = articulo.BiografiaCortaDestacado,
                UrlFotoDestacado = articulo.UrlFotoDestacado,
                UrlImagenPrincipal = articulo.UrlImagenPrincipal,
                FotosExistentes = articulo.Fotos?.OrderBy(f => f.Orden).ToList()
            };

            return View("CrearEditarArticulo", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarArticulo(ArticuloViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si falla la validación, recarga las fotos existentes para no perderlas en la vista
                var articuloOriginal = await _articuloService.GetArticuloByIdAsync(model.Id);
                model.FotosExistentes = articuloOriginal?.Fotos?.OrderBy(f => f.Orden).ToList() ?? new List<ArticuloFoto>();
                return View("CrearEditarArticulo", model);
            }

            var sanitizer = new HtmlSanitizer();
            model.ContenidoTexto = sanitizer.Sanitize(model.ContenidoTexto);
            model.BiografiaCortaDestacado = sanitizer.Sanitize(model.BiografiaCortaDestacado);

            var articulo = new Articulo
            {
                Id = model.Id,
                Titulo = model.Titulo,
                Resumen = model.Resumen,
                ContenidoTexto = model.ContenidoTexto,
                Categoria = model.Categoria,
                Estado = model.Estado,
                NombrePersonaDestacada = model.NombrePersonaDestacada,
                BiografiaCortaDestacado = model.BiografiaCortaDestacado,
                // Las URLs existentes se mantienen si no hay nuevo archivo subido (lógica en el servicio)
            };

            await _articuloService.UpdateArticuloAsync(
                articulo,
                model.FotoDestacadoFile,
                model.ImagenPrincipalFile,
                model.FotosContenidoFiles?.ToList());

            TempData["SuccessMessage"] = "Artículo actualizado exitosamente.";
            return RedirectToAction(nameof(Articulos));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarArticulo(int id)
        {
            await _articuloService.DeleteArticuloAsync(id);
            TempData["SuccessMessage"] = "Artículo eliminado exitosamente.";
            return RedirectToAction(nameof(Articulos));
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleEstudiante(int id)
        {
            try
            {
                var estudianteDto = await _usuarioService.ObtenerEstudianteDetalleAsync(id);
                if (estudianteDto == null)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado" });
                }

                return Json(new
                {
                    success = true,
                    estudiante = new
                    {
                        id = estudianteDto.Id,
                        nombre = estudianteDto.Nombre,
                        apellido = estudianteDto.Apellido,
                        email = estudianteDto.Email,
                        cedula = estudianteDto.Cedula,
                        whatsApp = estudianteDto.WhatsApp,
                        fechaNacimiento = estudianteDto.FechaNacimiento,
                        esMenorEdad = estudianteDto.EsMenorEdad,
                        nombreRepresentante = estudianteDto.NombreRepresentante,
                        cedulaRepresentante = estudianteDto.CedulaRepresentante,
                        experienciaTatuaje = estudianteDto.ExperienciaTatuaje,
                        atendidoPor = estudianteDto.AtendidoPor,
                        fechaCreacion = estudianteDto.FechaCreacion,
                        urlImagen = estudianteDto.UrlImagen,
                        cursosComprados = estudianteDto.CursosComprados,
                        clasesInscritas = estudianteDto.ClasesInscritas,
                        pedidos = estudianteDto.Pedidos.Select(p => new
                        {
                            id = p.Id,
                            fechaPedido = p.FechaPedido,
                            total = p.Total,
                            estado = p.Estado,
                            metodoPago = p.MetodoPago,
                            referenciaPago = p.ReferenciaPago
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EliminarFotoContenido(int fotoId, int articuloId)
        {
            await _articuloService.DeleteFotoContenidoAsync(fotoId);
            TempData["SuccessMessage"] = "Foto de contenido eliminada.";
            return RedirectToAction(nameof(EditarArticulo), new { id = articuloId });
        }
        [HttpPost]
        public async Task<IActionResult> GuardarArticulo(int id)
        {
            await _articuloService.UpdateArticuloEstadoAsync(id, "Guardado");
            TempData["SuccessMessage"] = "El artículo ha sido movido a **Guardado** y está listo para revisión.";
            return RedirectToAction(nameof(Articulos));
        }

        [HttpPost]
        public async Task<IActionResult> PublicarArticulo(int id)
        {
            await _articuloService.UpdateArticuloEstadoAsync(id, "Publicado");
            TempData["SuccessMessage"] = "¡El artículo ha sido **Publicado** exitosamente!";
            return RedirectToAction(nameof(Articulos));
        }

        [HttpPost]
        public async Task<IActionResult> VolverABorrador(int id)
        {
            await _articuloService.UpdateArticuloEstadoAsync(id, "Borrador");
            TempData["SuccessMessage"] = "El artículo ha vuelto a **Borrador** y está listo para editar.";
            return RedirectToAction(nameof(Articulos));
        }
        // ==================== GESTIÓN DE PAQUETES ====================

        [HttpGet]
        public async Task<IActionResult> GestionarPaquetes()
        {
            var paquetes = await _paqueteService.ObtenerTodosConDetallesAsync();

            var viewModel = paquetes.Select(p => new PaqueteListaViewModel
            {
                Id = p.Id,
                Titulo = p.Titulo,
                DescripcionCorta = p.DescripcionCorta,
                UrlImagen = p.UrlImagen,
                Precio = p.Precio,
                EsDestacado = p.EsDestacado,
                FechaPublicacion = p.FechaPublicacion,
                CantidadItems = (p.PaqueteCursos?.Count ?? 0) + (p.PaqueteProductos?.Count ?? 0),
                PrecioTotalItems = (p.PaqueteCursos?.Sum(pc => pc.Curso?.Precio ?? 0) ?? 0) +
                                  (p.PaqueteProductos?.Sum(pp => pp.Producto?.Precio ?? 0) ?? 0),
                Ahorro = ((p.PaqueteCursos?.Sum(pc => pc.Curso?.Precio ?? 0) ?? 0) +
                          (p.PaqueteProductos?.Sum(pp => pp.Producto?.Precio ?? 0) ?? 0)) - p.Precio
            }).ToList();

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerPaquetePorId(int id)
        {
            var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(id);
            if (paquete == null)
                return Json(new { success = false, message = "Paquete no encontrado" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = paquete.Id,
                    titulo = paquete.Titulo,
                    descripcionCorta = paquete.DescripcionCorta ?? "",
                    descripcionLarga = paquete.DescripcionLarga ?? "",
                    precio = paquete.Precio,
                    urlImagen = paquete.UrlImagen ?? "",
                    esDestacado = paquete.EsDestacado,
                    esRecomendado = paquete.EsRecomendado,
                    cursosIds = paquete.PaqueteCursos?.Select(pc => pc.CursoId).ToList() ?? new List<int>(),
                    productosIds = paquete.PaqueteProductos?.Select(pp => pp.ProductoId).ToList() ?? new List<int>()
                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> CrearPaquete()
        {
            var titulo = Request.Form["Titulo"].ToString();
            var descripcionCorta = Request.Form["DescripcionCorta"].ToString();
            var descripcionLarga = Request.Form["DescripcionLarga"].ToString();
            var precio = decimal.Parse(Request.Form["Precio"].ToString());
            var esDestacado = Request.Form["EsDestacado"].ToString() == "true";
            var esRecomendado = Request.Form["EsRecomendado"].ToString() == "true";
            var imagenArchivo = Request.Form.Files["imagenArchivo"];

            // Validación básica
            if (string.IsNullOrEmpty(titulo))
                ModelState.AddModelError("Titulo", "El título es obligatorio");

            if (precio < 0)
                ModelState.AddModelError("Precio", "El precio debe ser mayor o igual a cero");

            if (imagenArchivo == null || imagenArchivo.Length == 0)
                ModelState.AddModelError("imagenArchivo", "La imagen es obligatoria");

            // Obtener los IDs de cursos y productos como arrays de strings
            var cursosIdsStrings = Request.Form["CursosSeleccionados"].ToList();
            var productosIdsStrings = Request.Form["ProductosSeleccionados"].ToList();

            List<int> cursosIds = new List<int>();
            List<int> productosIds = new List<int>();

            foreach (var idStr in cursosIdsStrings)
            {
                if (int.TryParse(idStr, out int id))
                    cursosIds.Add(id);
            }

            foreach (var idStr in productosIdsStrings)
            {
                if (int.TryParse(idStr, out int id))
                    productosIds.Add(id);
            }


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList());
                return Json(new { success = false, errors });
            }

            var sanitizer = new HtmlSanitizer();
            descripcionCorta = sanitizer.Sanitize(descripcionCorta ?? string.Empty);
            descripcionLarga = sanitizer.Sanitize(descripcionLarga ?? string.Empty);

            var paquete = new Paquete
            {
                Titulo = titulo,
                DescripcionCorta = descripcionCorta,
                DescripcionLarga = descripcionLarga,
                Precio = precio,
                UrlImagen = string.Empty,
                EsDestacado = esDestacado,
                EsRecomendado = esRecomendado
            };

            await _paqueteService.CrearAsync(paquete, cursosIds, productosIds);

            // Guardar imagen
            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "paquetes", paquete.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(imagenArchivo.FileName);
                var fileName = $"imagen{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenArchivo.CopyToAsync(stream);
                }

                paquete.UrlImagen = $"/paquetes/{paquete.Id}/{fileName}";
                await _paqueteService.ActualizarSoloImagenAsync(paquete);
            }

            // Crear VentaItem para el paquete
            await _ventaItemService.CrearVentaItemAsync("Paquete", null, null, paquete.Precio, paquete.Id);

            return Json(new { success = true, message = "Paquete creado con éxito" });
        }

        [HttpPost]
        public async Task<IActionResult> EditarPaquete()
        {
            var id = int.Parse(Request.Form["Id"].ToString());
            var titulo = Request.Form["Titulo"].ToString();
            var descripcionCorta = Request.Form["DescripcionCorta"].ToString();
            var descripcionLarga = Request.Form["DescripcionLarga"].ToString();
            var precio = decimal.Parse(Request.Form["Precio"].ToString());
            var esDestacado = Request.Form["EsDestacado"].ToString() == "true";
            var esRecomendado = Request.Form["EsRecomendado"].ToString() == "true";
            var imagenArchivo = Request.Form.Files["imagenArchivo"];

            var sanitizer = new HtmlSanitizer();
            descripcionCorta = sanitizer.Sanitize(descripcionCorta ?? string.Empty);
            descripcionLarga = sanitizer.Sanitize(descripcionLarga ?? string.Empty);

            var paqueteExistente = await _paqueteService.ObtenerPorIdAsync(id);
            if (paqueteExistente == null)
                return Json(new { success = false, message = "Paquete no encontrado" });

            paqueteExistente.Titulo = titulo;
            paqueteExistente.DescripcionCorta = descripcionCorta;
            paqueteExistente.DescripcionLarga = descripcionLarga;
            paqueteExistente.Precio = precio;
            paqueteExistente.EsDestacado = esDestacado;
            paqueteExistente.EsRecomendado = esRecomendado;

            // Parsear los IDs de cursos y productos
            var cursosIdsStrings = Request.Form["CursosSeleccionados"].ToList();
            var productosIdsStrings = Request.Form["ProductosSeleccionados"].ToList();

            List<int> cursosIds = new List<int>();
            List<int> productosIds = new List<int>();

            foreach (var idStr in cursosIdsStrings)
            {
                if (int.TryParse(idStr, out int cursoId))
                    cursosIds.Add(cursoId);
            }

            foreach (var idStr in productosIdsStrings)
            {
                if (int.TryParse(idStr, out int productoId))
                    productosIds.Add(productoId);
            }

            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
                if (!string.IsNullOrEmpty(paqueteExistente.UrlImagen))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", paqueteExistente.UrlImagen.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "paquetes", paqueteExistente.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(imagenArchivo.FileName);
                var fileName = $"imagen{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenArchivo.CopyToAsync(stream);
                }

                paqueteExistente.UrlImagen = $"/paquetes/{paqueteExistente.Id}/{fileName}";
            }

            await _paqueteService.ActualizarAsync(paqueteExistente, cursosIds, productosIds);

            // Actualizar VentaItem
            var ventaItem = await _ventaItemService.ObtenerVentaItemPorPaqueteIdAsync(paqueteExistente.Id);
            if (ventaItem != null)
            {
                ventaItem.Precio = paqueteExistente.Precio;
                await _ventaItemService.ActualizarVentaItemAsync(ventaItem);
            }

            return Json(new { success = true, message = "Paquete actualizado con éxito" });
        }
        public async Task<IActionResult> EliminarPaquete(int id)
        {
            await _paqueteService.EliminarAsync(id);
            return Json(new { success = true, message = "Paquete eliminado con éxito" });
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerCursosProductosParaPaquete(int? paqueteId = null)
        {
            var cursos = await _cursoService.ObtenerTodosLosCursosAsync();
            var productos = await _productoService.GetAllAsync();

            return Json(new
            {
                success = true,
                cursos = cursos.Select(c => new { id = c.Id, titulo = c.Titulo, precio = c.Precio }),
                productos = productos.Select(p => new { id = p.Id, titulo = p.Titulo, precio = p.Precio })
            });
        }
        [HttpPost]
        public async Task<IActionResult> DestacarPaquete(int id)
        {
            var result = await _paqueteService.DestacarAsync(id);
            if (result)
                return Json(new { success = true, message = "Paquete destacado con éxito" });
            return Json(new { success = false, message = "Error al destacar el paquete" });
        }

        [HttpPost]
        public async Task<IActionResult> QuitarDestacadoPaquete(int id)
        {
            var result = await _paqueteService.QuitarDestacadoAsync(id);
            if (result)
                return Json(new { success = true, message = "Paquete ya no está destacado" });
            return Json(new { success = false, message = "Error al quitar destacado" });
        }

    }
}