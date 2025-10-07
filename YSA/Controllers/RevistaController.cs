using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Enums;
using YSA.Core.Interfaces;
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;

namespace YSA.Web.Controllers
{
    public class RevistaController : Controller
    {
        private readonly ICursoService _cursoService;
        private readonly IModuloService _moduloService;
        private readonly ILeccionService _leccionService;
        private readonly IPedidoService _pedidoService;
        private readonly IVentaItemService _ventaItemService;
        private readonly IArtistaService _artistaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEventoService _eventoService;
        public RevistaController(ICursoService cursoService, IModuloService moduloService, ILeccionService leccionService, IPedidoService pedidoService, UserManager<Usuario> userManager, IVentaItemService ventaItemService, IArtistaService artistaService, IEventoService eventoService)
        {
            _cursoService = cursoService;
            _moduloService = moduloService;
            _leccionService = leccionService;
            _pedidoService = pedidoService;
            _userManager = userManager;
            _ventaItemService = ventaItemService;
            _artistaService = artistaService;
            _eventoService = eventoService;
        }

        public async Task<IActionResult> Index()
        {
            var artistas = await _artistaService.GetAllArtistasAsync();

            // Obtenemos los eventos con TipoEventoId = 1 (Elevarte)
            var eventos = await _eventoService.GetEventosByTipoIdAsync(1);

            var viewModel = new RevistaIndexViewModel
            {
                Artistas = artistas,
                EventosElevarte = eventos // Pasa los eventos al ViewModel
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> DetalleEvento(int id)
        {
            var evento = await _eventoService.GetEventoByIdAsync(id);

            if (evento == null || !evento.EstaActivo) 
            {
                return NotFound();
            }

            var viewModel = new EventoDetalleViewModel
            {
                Id = evento.Id,
                Titulo = evento.Titulo,
                Descripcion = evento.Descripcion, 
                FechaEvento = evento.FechaEvento,
                Lugar = evento.Lugar,
                UrlImagen = evento.UrlImagen,
                EsDestacado = evento.EsDestacado,
                TipoEventoNombre = evento.TipoEvento?.NombreTipo,
                TipoEventoPlataforma = evento.TipoEvento?.Plataforma,
            };

            return View(viewModel);
        }
    }
}