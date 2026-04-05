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
        private readonly IArtistaService _artistaService;
        private readonly IEventoService _eventoService;
        private readonly ICursoService _cursoService; // Añadir servicio de cursos

        public HomeController(ILogger<HomeController> logger,
            SignInManager<Usuario> signInManager,
            IEventoService eventoService,
            IArtistaService artistaService,
            ICursoService cursoService) // Inyectar servicio de cursos
        {
            _logger = logger;
            _signInManager = signInManager;
            _eventoService = eventoService;
            _artistaService = artistaService;
            _cursoService = cursoService;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener eventos existentes (por si quieres mantenerlos)
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
                        Lugar = e.Lugar,
                        Descripcion = e.Descripcion
                    }).ToList()
                );

            // ==================== NUEVO: Obtener clases presenciales disponibles ====================
            var clasesDisponibles = await ObtenerClasesDisponiblesAsync();

            var artistas = await _artistaService.GetAllArtistasAsync();

            var viewModel = new HomeViewModel
            {
                EventosCategorizados = eventosCategorizados,
                Artistas = artistas,
                ClasesDisponibles = clasesDisponibles // Nueva propiedad
            };

            return View(viewModel);
        }

        // Método para obtener clases con vacantes disponibles
        private async Task<List<ClaseHomeViewModel>> ObtenerClasesDisponiblesAsync()
        {
            // Obtener todos los cursos presenciales
            var cursosPresenciales = await _cursoService.ObtenerCursosPresencialesAsync();

            var todasLasClases = new List<ClaseHomeViewModel>();

            foreach (var curso in cursosPresenciales)
            {
                var clases = await _cursoService.ObtenerClasesPorCursoIdAsync(curso.Id);

                // Filtrar solo clases programadas con fecha futura y con vacantes disponibles
                var clasesFiltradas = clases
                    .Where(c => c.Estado == "Programada"
                                && c.FechaHoraInicio > DateTime.Now
                                && (c.Inscripciones == null || c.Inscripciones.Count < c.CapacidadMaxima))
                    .Select(c => new ClaseHomeViewModel
                    {
                        Id = c.Id,
                        CursoId = curso.Id,
                        CursoTitulo = curso.Titulo,
                        Titulo = c.Titulo,
                        Descripcion = c.Descripcion,
                        FechaHoraInicio = c.FechaHoraInicio,
                        FechaHoraFin = c.FechaHoraFin,
                        Lugar = c.Lugar,
                        CapacidadMaxima = c.CapacidadMaxima,
                        VacantesDisponibles = c.CapacidadMaxima - (c.Inscripciones?.Count ?? 0),
                        UrlImagen = curso.UrlImagen,
                        Precio = curso.Precio
                    });

                todasLasClases.AddRange(clasesFiltradas);
            }

            // Ordenar por fecha más próxima y tomar las primeras 12
            return todasLasClases
                .OrderBy(c => c.FechaHoraInicio)
                .Take(12)
                .ToList();
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