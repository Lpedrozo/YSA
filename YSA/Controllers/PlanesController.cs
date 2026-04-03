// YSA.Web.Controllers/PlanesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Interfaces;
using YSA.Web.Models.ViewModels;
using System.Security.Claims;

namespace YSA.Web.Controllers
{
    public class PlanesController : Controller
    {
        private readonly IPlanSuscripcionService _planService;

        public PlanesController(IPlanSuscripcionService planService)
        {
            _planService = planService;
        }
        [Authorize(Roles = "Administrador, Asistente")]
        // GET: /Planes
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public IActionResult Catalogo()
        {
            return View();
        }

        [Authorize(Roles = "Administrador, Asistente")]

        // GET: /Planes/Listar
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                var planes = await _planService.GetAllPlanesAsync();
                var viewModels = planes.Select(p => new PlanSuscripcionViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    DuracionDias = p.DuracionDias,
                    LimitePublicaciones = p.LimitePublicaciones,
                    ComisionPorcentaje = p.ComisionPorcentaje,
                    TieneVisibilidadPrioritaria = p.TieneVisibilidadPrioritaria,
                    Orden = p.Orden,
                    Activo = p.Activo,
                    PermitePromocionesExtras = p.PermitePromocionesExtras,
                    MaxPromocionesSimultaneas = p.MaxPromocionesSimultaneas,
                    DescuentoComisionAdicional = p.DescuentoComisionAdicional
                }).OrderBy(p => p.Orden).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar los planes: " + ex.Message });
            }
        }
        [Authorize(Roles = "Administrador, Asistente")]

        // GET: /Planes/Obtener/{id}
        [HttpGet]
        public async Task<IActionResult> Obtener(int id)
        {
            try
            {
                var plan = await _planService.GetPlanByIdAsync(id);
                if (plan == null)
                {
                    return Json(new { success = false, message = "Plan no encontrado." });
                }

                var viewModel = new PlanSuscripcionViewModel
                {
                    Id = plan.Id,
                    Nombre = plan.Nombre,
                    Descripcion = plan.Descripcion,
                    Precio = plan.Precio,
                    DuracionDias = plan.DuracionDias,
                    LimitePublicaciones = plan.LimitePublicaciones,
                    ComisionPorcentaje = plan.ComisionPorcentaje,
                    TieneVisibilidadPrioritaria = plan.TieneVisibilidadPrioritaria,
                    Orden = plan.Orden,
                    Activo = plan.Activo,
                    PermitePromocionesExtras = plan.PermitePromocionesExtras,
                    MaxPromocionesSimultaneas = plan.MaxPromocionesSimultaneas,
                    DescuentoComisionAdicional = plan.DescuentoComisionAdicional
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener el plan: " + ex.Message });
            }
        }
        [Authorize(Roles = "Administrador, Asistente")]

        // POST: /Planes/Crear
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] PlanSuscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var planDto = new Core.DTOs.PlanSuscripcionDto
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Precio = model.Precio,
                    DuracionDias = model.DuracionDias,
                    LimitePublicaciones = model.LimitePublicaciones,
                    ComisionPorcentaje = model.ComisionPorcentaje,
                    TieneVisibilidadPrioritaria = model.TieneVisibilidadPrioritaria,
                    Orden = model.Orden,
                    PermitePromocionesExtras = model.PermitePromocionesExtras,
                    MaxPromocionesSimultaneas = model.MaxPromocionesSimultaneas,
                    DescuentoComisionAdicional = model.DescuentoComisionAdicional
                };

                var created = await _planService.CreatePlanAsync(planDto, usuarioId);

                return Json(new { success = true, message = "Plan creado exitosamente.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear el plan: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] PlanSuscripcionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var planDto = new Core.DTOs.PlanSuscripcionDto
                {
                    Id = model.Id,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Precio = model.Precio,
                    DuracionDias = model.DuracionDias,
                    LimitePublicaciones = model.LimitePublicaciones,
                    ComisionPorcentaje = model.ComisionPorcentaje,
                    TieneVisibilidadPrioritaria = model.TieneVisibilidadPrioritaria,
                    Orden = model.Orden,
                    PermitePromocionesExtras = model.PermitePromocionesExtras,
                    MaxPromocionesSimultaneas = model.MaxPromocionesSimultaneas,
                    DescuentoComisionAdicional = model.DescuentoComisionAdicional
                };

                var updated = await _planService.UpdatePlanAsync(planDto, usuarioId);

                return Json(new { success = true, message = "Plan actualizado exitosamente.", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el plan: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var result = await _planService.DeletePlanAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo eliminar el plan. Verifique que exista." });
                }

                return Json(new { success = true, message = "Plan eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el plan: " + ex.Message });
            }
        }

        [Authorize(Roles = "Administrador, Asistente")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _planService.TogglePlanStatusAsync(id, usuarioId);

                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo cambiar el estado del plan." });
                }

                return Json(new { success = true, message = "Estado del plan actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el estado: " + ex.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Artista, Administrador, Asistente")]
        public async Task<IActionResult> ListarActivos()
        {
            try
            {
                var planes = await _planService.GetActivePlanesAsync();
                var viewModels = planes.Select(p => new PlanSuscripcionViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    DuracionDias = p.DuracionDias,
                    LimitePublicaciones = p.LimitePublicaciones,
                    ComisionPorcentaje = p.ComisionPorcentaje,
                    TieneVisibilidadPrioritaria = p.TieneVisibilidadPrioritaria,
                    Orden = p.Orden,
                    Activo = p.Activo,
                    PermitePromocionesExtras = p.PermitePromocionesExtras,
                    MaxPromocionesSimultaneas = p.MaxPromocionesSimultaneas,
                    DescuentoComisionAdicional = p.DescuentoComisionAdicional
                }).OrderBy(p => p.Precio).ToList();

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar los planes: " + ex.Message });
            }
        }

    }
}