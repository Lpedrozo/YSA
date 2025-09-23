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
            // Obtener los eventos de la academia (TipoEventoId = 5)
            var eventosAcademia = await _eventoService.GetEventosByTipoIdAsync(5);

            // Obtener algunos artistas (ejemplo: los primeros 3 o 4)
            var artistas = (await _artistaService.GetAllArtistasAsync()).Take(4);

            var viewModel = new HomeViewModel
            {
                EventosAcademia = eventosAcademia,
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