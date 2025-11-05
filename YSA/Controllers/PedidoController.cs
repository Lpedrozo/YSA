using Microsoft.AspNetCore.Mvc;
using YSA.Core.Services;
using YSA.Core.Entities;
using System.Threading.Tasks;
using System.Security.Claims;
using YSA.Web.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using YSA.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;
    private readonly IVentaItemService _ventaItemService;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly INotificacionService _notificacionService;
    private readonly UserManager<Usuario> _userManager;

    public PedidoController(
        IPedidoService pedidoService,
        IVentaItemService ventaItemService,
        IExchangeRateService exchangeRateService,
        INotificacionService notificacionService,
        UserManager<Usuario> userManager)
    {
        _pedidoService = pedidoService;
        _ventaItemService = ventaItemService;
        _exchangeRateService = exchangeRateService;
        _notificacionService = notificacionService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> IniciarCompra(int cursoId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Cuenta", new { returnUrl = Url.Action("Detalles", "Curso", new { id = cursoId }) });
        }

        var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(estudianteId))
        {
            return Unauthorized("Usuario no autenticado.");
        }

        // Obtener el VentaItem asociado al curso
        var ventaItem = await _ventaItemService.ObtenerVentaItemPorCursoIdAsync(cursoId);
        if (ventaItem == null)
        {
            return NotFound("El curso no está disponible para la venta.");
        }

        // Crear el pedido usando el VentaItem
        var pedido = await _pedidoService.CrearPedidoAsync(Convert.ToInt32(estudianteId), new List<int> { ventaItem.Id });

        return RedirectToAction("Confirmar", new { pedidoId = pedido.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmar(int pedidoId)
    {
        var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(estudianteId))
        {
            return Unauthorized();
        }

        // Pedido ahora incluye la navegación a VentaItem, Producto y Curso
        var pedido = await _pedidoService.ObtenerPedidoConItemsYVentaItemsAsync(pedidoId);
        if (pedido == null || pedido.EstudianteId != Convert.ToInt32(estudianteId))
        {
            return NotFound("Pedido no encontrado o no autorizado.");
        }

        var tasaHoy = await _exchangeRateService.GetTasaToday();

        if (!tasaHoy.HasValue || tasaHoy.Value <= 0)
        {
            TempData["ErrorMessage"] = "No se pudo obtener la tasa de cambio oficial. Intente más tarde.";
            return RedirectToAction("Index");
        }

        var viewModel = new ConfirmacionPagoViewModel
        {
            PedidoId = pedido.Id,
            Total = pedido.Total,
            TasaBCV = tasaHoy,
            Articulos = pedido.PedidoItems.Select(item =>
            {
                string titulo = item.VentaItem.Curso?.Titulo ?? item.VentaItem.Producto?.Titulo;
                return new ArticuloPedidoViewModel
                {
                    TituloItem = titulo,
                    Precio = item.PrecioUnidad
                };
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ProcesarPago(int pedidoId, string metodoPago, string referenciaPago, IFormFile comprobanteArchivo)
    {
        var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(estudianteId))
        {
            return Unauthorized();
        }

        var pedido = await _pedidoService.ObtenerPedidoConDetallesCompletosAsync(pedidoId);
        if (pedido == null || pedido.EstudianteId != Convert.ToInt32(estudianteId))
        {
            return NotFound("Pedido no encontrado o no autorizado.");
        }

        if (comprobanteArchivo == null || comprobanteArchivo.Length == 0)
        {
            TempData["Error"] = "Debe subir un comprobante de pago.";
            return RedirectToAction("Confirmar", new { pedidoId });
        }

        try
        {
            var fileName = Path.GetFileName(comprobanteArchivo.FileName);
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await comprobanteArchivo.CopyToAsync(stream);
            }

            var urlComprobante = $"/comprobantes/{fileName}";

            var pago = new Pago
            {
                PedidoId = pedido.Id,
                MetodoPago = metodoPago,
                ReferenciaPago = referenciaPago,
                UrlComprobante = urlComprobante,
                FechaPago = DateTime.UtcNow
            };
            await _pedidoService.RegistrarPagoAsync(pago);

            await _pedidoService.ActualizarEstadoPedidoAsync(pedido.Id, "Validando");

            // *** NUEVO: ENVIAR NOTIFICACIONES ***
            await EnviarNotificacionesCompraCurso(pedido);

            TempData["PagoRegistrado"] = true;
            return RedirectToAction("Index", "Curso");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Ocurrió un error al procesar su pago. Intente de nuevo.";
            return RedirectToAction("Confirmar", new { pedidoId });
        }
    }

    private async Task EnviarNotificacionesCompraCurso(Pedido pedido)
    {
        var usuarioId = pedido.EstudianteId;

        // Obtener información del curso comprado
        var cursoItem = pedido.PedidoItems.FirstOrDefault()?.VentaItem?.Curso;
        if (cursoItem != null)
        {
            // 1. Notificar al usuario que su pago está en validación
            await _notificacionService.CrearNotificacionPagoPendienteAsync(
                usuarioId,
                pedido.Id,
                pedido.Total
            );

            // 2. Notificar a los administradores sobre el nuevo pedido
            await _notificacionService.CrearNotificacionNuevoPedidoAsync(
                pedido.Id,
                pedido.Total,
                $"Curso: {cursoItem.Titulo}"
            );
        }
    }
}