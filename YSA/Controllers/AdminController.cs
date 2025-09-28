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
using YSA.Web.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YSA.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ICursoService _cursoService;
        private readonly IModuloService _moduloService;
        private readonly ILeccionService _leccionService;
        private readonly IPedidoService _pedidoService;
        private readonly IVentaItemService _ventaItemService;
        private readonly IArtistaService _artistaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEventoService _eventoService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IProductoService _productoService;

        public AdminController(ICursoService cursoService, 
            IModuloService moduloService, 
            ILeccionService leccionService, 
            IPedidoService pedidoService,
            UserManager<Usuario> userManager, 
            IVentaItemService ventaItemService, 
            IArtistaService artistaService, 
            IEventoService eventoService, 
            IWebHostEnvironment hostingEnvironment, 
            IProductoService productoService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
            _artistaService = artistaService;
            _eventoService = eventoService;
            _hostingEnvironment = hostingEnvironment;
            _productoService = productoService;
        }

        public IActionResult Panel()
        {
            var model = new DashboardViewModel
            {
                TotalCursos = 120, // Reemplazar con datos reales
                TotalEstudiantes = 540, // Reemplazar con datos reales
                PedidosPendientes = 8 // Reemplazar con datos reales
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
                InstructorId = c.InstructorId,
                Instructor = c.Instructor,
                CategoriasSeleccionadas = c.CursoCategorias.Select(cc => cc.CategoriaId).ToArray()
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

        [HttpPost]
        public async Task<IActionResult> CrearCurso([FromForm] CursoViewModel model)
        {
            // El model binding ahora extrae los campos de texto del formulario.
            // IFormFile debe ser un parámetro separado.
            IFormFile imagenArchivo = Request.Form.Files["imagenArchivo"];
            var categoriasSeleccionadas = Request.Form["CategoriasSeleccionadas"].Select(int.Parse).ToArray();

            // La validación del modelo ahora debe hacerse manualmente para la imagen y las categorías.
            if (string.IsNullOrEmpty(model.Titulo)) ModelState.AddModelError("Titulo", "El título es obligatorio.");
            if (model.Precio <= 0) ModelState.AddModelError("Precio", "El precio debe ser mayor a cero.");
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
            };

            // El model.CategoriasSeleccionadas del viewmodel ya no se usa, ya que la lista se extrae manualmente
            // y se le pasa directamente a la capa de servicio
            await _cursoService.CrearCursoAsync(curso, categoriasSeleccionadas);

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
            if (model.Precio <= 0)
            {
                ModelState.AddModelError("Precio", "El precio debe ser mayor a cero.");
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
        public async Task<IActionResult> PedidosPendientes()
        {
            var pedidosPendientes = await _pedidoService.ObtenerPedidosPorEstadoAsync("Validando");

            var viewModel = new List<PedidoPendienteViewModel>();

            foreach (var pedido in pedidosPendientes)
            {
                var estudiante = await _userManager.FindByIdAsync(pedido.EstudianteId.ToString());

                viewModel.Add(new PedidoPendienteViewModel
                {
                    PedidoId = pedido.Id,
                    NombreEstudiante = estudiante?.Nombre ?? "Estudiante Desconocido", // Muestra un nombre por defecto si no se encuentra
                    FechaPedido = pedido.FechaPedido,
                    Total = pedido.Total
                });
            }

            // 4. Pasa la lista de ViewModels a la vista
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AprobarPedido(int pedidoId)
        {
            // Llama al servicio para cambiar el estado y otorgar acceso
            await _pedidoService.AprobarPedidoYOtorgarAccesoAsync(pedidoId);
            return RedirectToAction(nameof(PedidosPendientes));
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
        public async Task<IActionResult> GestionarEstudiantes()
        {
            var estudiantes = await _userManager.GetUsersInRoleAsync("Estudiante");

            var viewModel = new List<EstudianteConCursosViewModel>();

            foreach (var estudiante in estudiantes)
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

                viewModel.Add(new EstudianteConCursosViewModel
                {
                    Id = estudiante.Id,
                    Nombre = estudiante.Nombre,
                    Email = estudiante.Email,
                    CursosComprados = cursosComprados.Distinct().ToList() 
                });
            }

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
                UrlImagen = a.Usuario.UrlImagen
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
    }
}