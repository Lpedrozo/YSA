// YSA.Web.Controllers/SuscripcionesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Interfaces;
using YSA.Web.Models.ViewModels;
using System.Security.Claims;
using YSA.Core.Services;

namespace YSA.Web.Controllers
{
    public class SuscripcionesController : Controller
    {
        private readonly ISuscripcionArtistaService _suscripcionService;
        private readonly IArtistaService _artistaService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SuscripcionesController(
            ISuscripcionArtistaService suscripcionService,
            IArtistaService artistaService,
            IWebHostEnvironment hostingEnvironment)
        {
            _suscripcionService = suscripcionService;
            _artistaService = artistaService;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "Administrador, Asistente")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Artista, Administrador, Asistente")]
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var dashboard = await _suscripcionService.GetDashboardDataAsync();
                return Json(new { success = true, data = dashboard });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar el dashboard: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpGet]
        public async Task<IActionResult> ListarPendientes()
        {
            try
            {
                var pendientes = await _suscripcionService.GetSuscripcionesPendientesValidacionAsync();
                return Json(new { success = true, data = pendientes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar suscripciones pendientes: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpGet]
        public async Task<IActionResult> ListarHistorial()
        {
            try
            {
                var todas = await _suscripcionService.GetAllSuscripcionesAsync();
                return Json(new { success = true, data = todas });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar el historial: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpPost]
        public async Task<IActionResult> Aprobar([FromBody] AprobarRechazarRequest request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                {
                    return Json(new { success = false, message = "ID de suscripción inválido." });
                }

                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _suscripcionService.AprobarSuscripcionAsync(request.Id, adminId, request.Notas ?? "");

                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo aprobar la suscripción." });
                }

                return Json(new { success = true, message = "Suscripción aprobada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al aprobar: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpPost]
        public async Task<IActionResult> Rechazar([FromBody] AprobarRechazarRequest request)
        {
            try
            {
                if (request == null || request.Id <= 0)
                {
                    return Json(new { success = false, message = "ID de suscripción inválido." });
                }

                if (string.IsNullOrWhiteSpace(request.Notas))
                {
                    return Json(new { success = false, message = "Debes especificar un motivo para el rechazo." });
                }

                var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _suscripcionService.RechazarSuscripcionAsync(request.Id, adminId, request.Notas);

                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo rechazar la suscripción." });
                }

                return Json(new { success = true, message = "Suscripción rechazada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al rechazar: " + ex.Message });
            }
        }

        // Clase para las solicitudes
        public class AprobarRechazarRequest
        {
            public int Id { get; set; }
            public string Notas { get; set; }
        }

        // POST: /Suscripciones/Solicitar - CORREGIDO (sin [FromBody])
        [HttpPost]
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public async Task<IActionResult> Solicitar([FromForm] SolicitarSuscripcionRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Json(new { success = false, message = "Usuario no autenticado." });
                }

                var artista = await _artistaService.ObtenerArtistaPorUsuarioIdAsync(usuarioId);
                if (artista == null)
                {
                    return Json(new { success = false, message = "No tienes un perfil de artista. Contacta al administrador." });
                }

                // Verificar si ya tiene una suscripción activa
                var tieneActiva = await _suscripcionService.TieneSuscripcionActivaAsync(artista.Id);
                if (tieneActiva)
                {
                    return Json(new { success = false, message = "Ya tienes una suscripción activa. No puedes solicitar otra hasta que venza." });
                }

                // Guardar el comprobante
                if (request.Comprobante == null || request.Comprobante.Length == 0)
                {
                    return Json(new { success = false, message = "Debes subir el comprobante de pago." });
                }

                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "comprobantes", "suscripciones", artista.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(request.Comprobante.FileName);
                var fileName = $"comprobante_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Comprobante.CopyToAsync(stream);
                }

                var comprobanteUrl = $"/comprobantes/suscripciones/{artista.Id}/{fileName}";

                // Crear la suscripción
                var suscripcion = await _suscripcionService.CreateSuscripcionAsync(artista.Id, request.PlanId, comprobanteUrl);

                return Json(new { success = true, message = "Solicitud de suscripción enviada. Espera la validación del administrador." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la solicitud: " + ex.Message });
            }
        }

        // GET: /Suscripciones/MisSuscripciones - Devuelve la vista HTML
        [HttpGet]
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public IActionResult MisSuscripciones()
        {
            return View();
        }

        // GET: /Suscripciones/ObtenerMisSuscripciones - Devuelve JSON
        [HttpGet]
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public async Task<IActionResult> ObtenerMisSuscripciones()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Json(new { success = true, data = new List<SuscripcionArtistaViewModel>() });
                }

                var artista = await _artistaService.ObtenerArtistaPorUsuarioIdAsync(usuarioId);
                if (artista == null)
                {
                    return Json(new { success = true, data = new List<SuscripcionArtistaViewModel>() });
                }

                var suscripciones = await _suscripcionService.GetSuscripcionesByArtistaAsync(artista.Id);
                var viewModels = suscripciones.Select(s => new SuscripcionArtistaViewModel
                {
                    Id = s.Id,
                    SnapshotNombre = s.SnapshotNombre,
                    SnapshotPrecio = s.SnapshotPrecio,
                    SnapshotDuracionDias = s.SnapshotDuracionDias,
                    SnapshotLimitePublicaciones = s.SnapshotLimitePublicaciones,
                    Estado = s.Estado,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin,
                    FechaCreacion = s.FechaCreacion,
                    NotasAdmin = s.NotasAdmin
                }).OrderByDescending(s => s.FechaCreacion).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar tus suscripciones: " + ex.Message });
            }
        }
        // POST: /Suscripciones/Renovar
        [HttpPost]
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public async Task<IActionResult> Renovar([FromForm] RenovarSuscripcionRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                {
                    return Json(new { success = false, message = "Usuario no autenticado." });
                }

                var artista = await _artistaService.ObtenerArtistaPorUsuarioIdAsync(usuarioId);
                if (artista == null)
                {
                    return Json(new { success = false, message = "No tienes un perfil de artista." });
                }

                // Guardar el comprobante
                if (request.Comprobante == null || request.Comprobante.Length == 0)
                {
                    return Json(new { success = false, message = "Debes subir el comprobante de pago." });
                }

                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "comprobantes", "suscripciones", artista.Id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(request.Comprobante.FileName);
                var fileName = $"renovacion_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Comprobante.CopyToAsync(stream);
                }

                var comprobanteUrl = $"/comprobantes/suscripciones/{artista.Id}/{fileName}";

                // Crear la nueva suscripción (renovación)
                var nuevaSuscripcion = await _suscripcionService.RenovarSuscripcionAsync(request.SuscripcionAnteriorId, request.PlanId, comprobanteUrl);

                return Json(new { success = true, message = "Solicitud de renovación enviada. Espera la validación del administrador." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la solicitud: " + ex.Message });
            }
        }

        public class RenovarSuscripcionRequest
        {
            public int SuscripcionAnteriorId { get; set; }
            public int PlanId { get; set; }
            public IFormFile Comprobante { get; set; }
        }
        public class SolicitarSuscripcionRequest
        {
            public int PlanId { get; set; }
            public IFormFile Comprobante { get; set; }
        }
    }
}