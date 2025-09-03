using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;

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
        private readonly UserManager<Usuario> _userManager;

        public AdminController(ICursoService cursoService, IModuloService moduloService, ILeccionService leccionService, IPedidoService pedidoService, UserManager<Usuario> userManager, IVentaItemService ventaItemService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
        }

        public IActionResult Panel()
        {
            return View();
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
                Titulo = c.Titulo,
                DescripcionCorta = c.DescripcionCorta,
                DescripcionLarga = c.DescripcionLarga,
                Precio = c.Precio,
                UrlImagen = c.UrlImagen,
                CategoriasSeleccionadas = c.CursoCategorias.Select(cc => cc.CategoriaId).ToArray()
            }).ToList();

            var categorias = await _cursoService.ObtenerTodasLasCategoriasAsync();
            var listaCategorias = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.NombreCategoria
            }).ToList();

            ViewBag.CategoriasCrear = listaCategorias;
            ViewBag.CategoriasEditar = listaCategorias;

            return View(viewModels);
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

            var curso = new Curso
            {
                Titulo = model.Titulo,
                DescripcionCorta = model.DescripcionCorta,
                DescripcionLarga = model.DescripcionLarga,
                Precio = model.Precio,
                UrlImagen = string.Empty
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
        public async Task<IActionResult> EditarCurso(CursoViewModel model, IFormFile imagenArchivo)
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
    }
}