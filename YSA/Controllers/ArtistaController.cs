using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Entities;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;

namespace YSA.Web.Controllers
{
    public class ArtistaController : Controller
    {
        private readonly ICursoService _cursoService;
        private readonly IModuloService _moduloService;
        private readonly ILeccionService _leccionService;
        private readonly IPedidoService _pedidoService;
        private readonly IVentaItemService _ventaItemService;
        private readonly IArtistaService _artistaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRecursoActividadService _recursoActividadService; // ¡NECESITAS ESTO!

        public ArtistaController(
            ICursoService cursoService,
            IModuloService moduloService,
            ILeccionService leccionService,
            IPedidoService pedidoService,
            UserManager<Usuario> userManager,
            IVentaItemService ventaItemService,
            IArtistaService artistaService,
            IWebHostEnvironment webHostEnvironment,
            IRecursoActividadService recursoActividadService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
            _artistaService = artistaService;
            _webHostEnvironment = webHostEnvironment;
            _recursoActividadService = recursoActividadService;
        }

        public async Task<IActionResult> GestionarPortafolio()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return NotFound();
            }

            string userIdString = usuario.Id.ToString();
            var artista = await _artistaService.ObtenerArtistaPorUsuarioIdAsync(userIdString);
            if (artista == null)
            {
                return NotFound();
            }

            var fotos = await _artistaService.ObtenerFotosPortafolioAsync(artista.Id);
            var viewModel = new PortafolioViewModel
            {
                ArtistaId = artista.Id,
                Fotos = fotos.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubirFoto(IFormFile archivo, string titulo)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return NotFound();

            var artista = await _artistaService.ObtenerArtistaPorUsuarioIdAsync(usuario.Id.ToString());
            if (artista == null) return NotFound();

            if (archivo == null || archivo.Length == 0)
            {
                return Json(new { success = false, errors = new { imagenArchivo = "El archivo no puede ser nulo o estar vacío." } });
            }

            try
            {
                var nombreArchivoUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);

                var artistaFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Artista", artista.Id.ToString());
                if (!Directory.Exists(artistaFolder))
                {
                    Directory.CreateDirectory(artistaFolder);
                }

                var rutaCompletaArchivo = Path.Combine(artistaFolder, nombreArchivoUnico);

                using (var fileStream = new FileStream(rutaCompletaArchivo, FileMode.Create))
                {
                    await archivo.CopyToAsync(fileStream);
                }

                await _artistaService.AgregarFotoPortafolioAsync(artista.Id, archivo.OpenReadStream(), nombreArchivoUnico, titulo);

                return Json(new { success = true, message = "Foto subida con éxito." });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, errors = new { imagenArchivo = new List<string> { ex.Message } } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new { imagenArchivo = new List<string> { $"Ocurrió un error inesperado al subir la foto: {ex.Message}" } } });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EliminarFoto(int fotoId)
        {
            try
            {
                var urlImagen = await _artistaService.EliminarFotoPortafolioAsync(fotoId);

                var rutaFisica = Path.Combine(_webHostEnvironment.WebRootPath, urlImagen.TrimStart('/'));

                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }

                return RedirectToAction(nameof(GestionarPortafolio));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(GestionarPortafolio));
            }
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerPortafolioJson(int artistaId)
        {
            var fotos = await _artistaService.ObtenerFotosPortafolioAsync(artistaId);
            return Json(fotos);
        }
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            // Usamos la tupla para obtener el artista y el portafolio (aunque el portafolio no se use en la vista final, la llamada del servicio puede ser necesaria)
            var (artista, fotos) = await _artistaService.ObtenerArtistaYPortafolioAsync(id);

            if (artista == null)
            {
                return NotFound(); // Artista no encontrado
            }

            var usuario = await _userManager.FindByIdAsync(artista.UsuarioId.ToString());
            if (usuario == null)
            {
                return NotFound("Usuario asociado al artista no encontrado.");
            }

            // 1. Obtener los cursos asociados al artista
            var cursos = await _artistaService.GetCursosByArtistaAsync(artista.Id);

            // 2. Mapear Entidades a ViewModel
            var viewModel = new ArtistaDetallesViewModel
            {
                Id = artista.Id,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Biografia = artista.Biografia,
                UrlFotoPerfil = usuario.UrlImagen,
                Portafolio = fotos ?? new List<ArtistaFoto>(),

                // 3. Asignar los cursos obtenidos al ViewModel
                Cursos = cursos ?? new List<Curso>()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> PreguntasPendientes()
        {
            var instructorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdString) || !int.TryParse(instructorIdString, out int instructorId))
            {
                return Unauthorized();
            }

            // 1. Obtiene las entidades del dominio (PreguntaRespuesta) desde el servicio
            var preguntasPendientes = await _cursoService.ObtenerPreguntasPendientesParaInstructorAsync(instructorId);

            // 2. Mapeo a ViewModel en el Controlador
            var viewModel = preguntasPendientes.Select(p => new PreguntaPendienteViewModel
            {
                Id = p.Id,
                CursoTitulo = p.Curso?.Titulo ?? "Curso Desconocido", // Asegura que las relaciones estén incluidas en el Repo/Service
                EstudianteNombre = p.Estudiante?.Nombre ?? "Estudiante Desconocido",
                Pregunta = p.Pregunta,
                FechaPregunta = p.FechaPregunta
            }).ToList();

            // 3. Pasa la lista de ViewModels a la vista
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponderPregunta(int preguntaId, string respuesta)
        {
            var instructorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdString) || !int.TryParse(instructorIdString, out int instructorId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(respuesta))
            {
                TempData["Error"] = "La respuesta no puede estar vacía.";
                return RedirectToAction(nameof(PreguntasPendientes));
            }

            var exito = await _cursoService.ResponderPreguntaAsync(preguntaId, respuesta, instructorId);

            if (exito)
            {
                TempData["Success"] = "Respuesta enviada con éxito.";
            }
            else
            {
                TempData["Error"] = "No se pudo responder la pregunta. Podría ya haber sido respondida o no existe.";
            }

            return RedirectToAction(nameof(PreguntasPendientes));
        }

        [HttpGet]
        public async Task<IActionResult> EntregasPendientes()
        {
            var instructorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdString) || !int.TryParse(instructorIdString, out int instructorId))
            {
                // Manejar usuario no autenticado o ID inválido
                return Unauthorized();
            }

            // 1. Obtener las entregas pendientes usando el nuevo servicio
            var entregasPendientes = await _recursoActividadService.ObtenerTareasPendientesParaInstructorAsync(instructorId);

            // 2. Mapear a ViewModel
            var viewModel = entregasPendientes.Select(e => new EntregaActividadPendienteViewModel // Necesitarás crear este ViewModel
            {
                EntregaId = e.Id,
                TipoEntidad = e.RecursoActividad?.TipoEntidad,
                ActividadTitulo = e.RecursoActividad?.Titulo ?? "Actividad Desconocida",
                CursoTitulo = e.RecursoActividad?.Titulo ?? "Curso Desconocido",
                EstudianteNombre = e.Estudiante?.Nombre ?? "Estudiante Desconocido",
                FechaEntrega = e.FechaEntrega,
                UrlArchivoEntrega = e.UrlArchivoEntrega,
                ComentarioEstudiante = e.ComentarioEstudiante,
                // Opcional: otros detalles necesarios para la vista (e.g., TipoArchivo)
            }).ToList();

            // 3. Pasa la lista de ViewModels a la vista
            return View(viewModel);
        }

        /// <summary>
        /// Procesa la calificación de una entrega de actividad.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalificarEntrega(int entregaId, decimal calificacion, string observacion)
        {
            var instructorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(instructorIdString) || !int.TryParse(instructorIdString, out int instructorId))
            {
                return Unauthorized();
            }

            // Validaciones básicas (puedes agregar más validación de rango 0-100 si aplicas un sistema numérico)
            if (string.IsNullOrWhiteSpace(observacion) || calificacion < 0)
            {
                TempData["Error"] = "La calificación y/o la observación son obligatorias o inválidas.";
                return RedirectToAction(nameof(EntregasPendientes));
            }

            // Llamar al servicio para calificar
            var exito = await _recursoActividadService.CalificarEntregaAsync(entregaId, instructorId, calificacion, observacion);

            if (exito)
            {
                TempData["Success"] = "Entrega calificada con éxito.";
            }
            else
            {
                TempData["Error"] = "No se pudo calificar la entrega. Asegúrate de que existe y que te pertenece como instructor.";
            }

            return RedirectToAction(nameof(EntregasPendientes));
        }
    }
}