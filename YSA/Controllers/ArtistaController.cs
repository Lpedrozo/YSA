using System;
using System.IO;
using System.Linq;
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

        public ArtistaController(
            ICursoService cursoService,
            IModuloService moduloService,
            ILeccionService leccionService,
            IPedidoService pedidoService,
            UserManager<Usuario> userManager,
            IVentaItemService ventaItemService,
            IArtistaService artistaService,
            IWebHostEnvironment webHostEnvironment)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
            _artistaService = artistaService;
            _webHostEnvironment = webHostEnvironment;
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

            // 2. Mapear Entidades a ViewModel
            var viewModel = new ArtistaDetallesViewModel
            {
                Id = artista.Id,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Biografia = artista.Biografia,
                UrlFotoPerfil = usuario.UrlImagen,
                Portafolio = fotos ?? new List<ArtistaFoto>(), 

            };

            return View(viewModel);
        }
    }
}