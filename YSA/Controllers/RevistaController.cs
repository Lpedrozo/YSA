// En YSA.Web.Controllers/RevistaController.cs

using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Web.Models.ViewModels;
using System.Collections.Generic;
using YSA.Core.Services;

namespace YSA.Web.Controllers
{
    public class RevistaController : Controller
    {
        private readonly IArticuloService _articuloService;
        private readonly IArtistaService _artistaService;
        private readonly IEventoService _eventoService;
        private readonly HtmlSanitizer _sanitizer;

        public RevistaController(IArticuloService articuloService, IArtistaService artistaService, IEventoService eventoService)
        {
            _articuloService = articuloService;
            _artistaService = artistaService;
            _eventoService = eventoService;
            _sanitizer = new HtmlSanitizer();
        }

        public async Task<IActionResult> Index()
        {
            var articulos = await _articuloService.GetAllArticulosAsync();

            // 1. FILTRO ÚNICO: Solo artículos en estado "Publicado"
            var articulosPublicados = articulos.Where(a => a.Estado == "Publicado")
                                               .OrderByDescending(a => a.FechaPublicacion)
                                               .ToList();

            // Lógica de Presentación: Los 4 más recientes son destacados. El resto son recientes.
            const int destacadosCount = 4;

            // 2. Separar en Destacados (Top 4 más recientes)
            var destacados = articulosPublicados
                                      .Take(destacadosCount)
                                      .ToList();

            // 3. Separar en Recientes (el resto, saltando los destacados)
            var recientes = articulosPublicados
                                     .Skip(destacadosCount)
                                     .ToList();

            var viewModel = new RevistaIndexViewModel
            {
                ArticulosDestacados = destacados,
                ArticulosRecientes = recientes
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetalleArticulo(int id)
        {
            var articulo = await _articuloService.GetArticuloByIdAsync(id);

            // Validación estricta: debe existir Y estar en estado "Publicado"
            if (articulo == null || articulo.Estado != "Publicado")
            {
                return NotFound();
            }

            var contenidoSanitizado = _sanitizer.Sanitize(articulo.ContenidoTexto ?? string.Empty);

            // Mapeo al ViewModel
            var viewModel = new ArticuloDetalleViewModel
            {
                Id = articulo.Id,
                Titulo = articulo.Titulo,
                Resumen = articulo.Resumen ?? string.Empty,
                ContenidoTexto = contenidoSanitizado,
                Categoria = articulo.Categoria ?? "General",
                FechaPublicacion = articulo.FechaPublicacion,
                UrlImagenPrincipal = articulo.UrlImagenPrincipal ?? string.Empty,
                NombrePersonaDestacada = articulo.NombrePersonaDestacada,
                BiografiaCortaDestacado = articulo.BiografiaCortaDestacado,
                UrlFotoDestacado = articulo.UrlFotoDestacado,
                FotosContenido = articulo.Fotos?.OrderBy(f => f.Orden).ToList() ?? new List<ArticuloFoto>()
            };

            return View(viewModel);
        }
    }
}