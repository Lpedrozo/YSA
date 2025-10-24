using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Core.Services;
using YSA.Web.Models;
using YSA.Web.Models.ViewModels;

namespace YSA.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IArtistaService _artistaService; // Inyecta el servicio de artistas
        private readonly IEventoService _eventoService; // Inyecta el servicio de eventos

        public HomeController(ILogger<HomeController> logger, SignInManager<Usuario> signInManager, IEventoService eventoService, IArtistaService artistaService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _eventoService = eventoService;
            _artistaService = artistaService;
        }

        public async Task<IActionResult> Index()
        {
            var eventosConferencia = await _eventoService.GetEventosByTipoIdAsync(1);
            var eventosGaleria = await _eventoService.GetEventosByTipoIdAsync(2);
            var eventosExposicion = await _eventoService.GetEventosByTipoIdAsync(3);
            var eventosTaller = await _eventoService.GetEventosByTipoIdAsync(4);
            var eventosSeminario = await _eventoService.GetEventosByTipoIdAsync(5);

            var todosLosEventos = eventosConferencia
                .Concat(eventosGaleria)
                .Concat(eventosExposicion)
                .Concat(eventosTaller)
                .Concat(eventosSeminario)
                .ToList();

            var eventosCategorizados = todosLosEventos
                .GroupBy(e => e.TipoEvento.NombreTipo) 
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new EventoHomeViewModel
                    {
                        Id = e.Id,
                        Titulo = e.Titulo,
                        TipoEventoNombre = e.TipoEvento.NombreTipo,
                        UrlImagen = e.UrlImagen,
                        FechaEvento = e.FechaEvento,
                        Lugar = e.Lugar
                    }).ToList()
                );

            var artistas = await _artistaService.GetAllArtistasAsync();

            var viewModel = new HomeViewModel
            {
                EventosCategorizados = eventosCategorizados,
                Artistas = artistas
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
        public IActionResult SobreNosotros()
        {
            return View();
        }
    }
}