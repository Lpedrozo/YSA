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
using YSA.Web.Models.ViewModels;
using System.IO;
using YSA.Core.Services;

namespace YSA.Web.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICompraService _compraService;
        private readonly IPedidoService _pedidoService;
        private readonly UserManager<Usuario> _userManager; 
        private readonly IExchangeRateService _exchangeRateService;

        public ProductoController(IProductoService productoService,
                                  ICompraService compraService,
                                  UserManager<Usuario> userManager,
                                  IPedidoService pedidoService,
                                  IExchangeRateService exchangeRateService)

        {
            _productoService = productoService;
            _compraService = compraService;
            _userManager = userManager;
            _pedidoService = pedidoService;
            _exchangeRateService = exchangeRateService;

        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            const int pageSize = 9; // Define cuántos productos quieres por página

            // 1. Obtener todos los productos
            var todosLosProductos = await _productoService.GetAllAsync();

            // 2. Aplicar filtro de búsqueda
            var productosFiltrados = todosLosProductos.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productosFiltrados = productosFiltrados.Where(p =>
                    p.Titulo.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.DescripcionCorta.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Aplicar Paginación
            var productosOrdenados = productosFiltrados.OrderByDescending(p => p.Id).ToList();
            var totalProductos = productosOrdenados.Count();
            var totalPaginas = (int)Math.Ceiling(totalProductos / (double)pageSize);

            // Asegurarse de que el número de página sea válido
            if (page < 1) page = 1;
            if (page > totalPaginas && totalPaginas > 0) page = totalPaginas;

            var productosPaginados = productosOrdenados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. Lógica de autenticación y estados de compra (se mantiene)
            var productosCompradosIds = new List<int>();
            var productosEnValidacionIds = new List<int>();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    productosCompradosIds = (await _compraService.GetPurchasedProductIdsAsync(user.Id)).ToList();
                    productosEnValidacionIds = (await _compraService.GetProductsInValidationIdsAsync(user.Id)).ToList();
                }
            }

            // 5. Crear y devolver el ViewModel
            var viewModel = new ProductosIndexViewModel
            {
                // Usamos los productos Paginados
                Productos = productosPaginados,
                ProductosCompradosIds = productosCompradosIds,
                ProductosEnValidacionIds = productosEnValidacionIds,

                // Asignamos datos de paginación
                PaginaActual = page,
                TotalPaginas = totalPaginas
            };

            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ComprarProducto(int productoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no autenticado.";
                return RedirectToAction("Index");
            }

            // Iniciar la compra y obtener el pedido
            var pedido = await _compraService.IniciarCompraProductoAsync(productoId, user.Id);

            if (pedido == null)
            {
                TempData["ErrorMessage"] = "El producto no existe o la compra no pudo iniciarse.";
                return RedirectToAction("Index");
            }

            // Redirigir a una nueva acción para el pago
            return RedirectToAction("Pagar", new { pedidoId = pedido.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Pagar(int pedidoId)
        {
            // Opcional: Validar que el pedido pertenezca al usuario actual y que esté en estado "Pendiente"
            var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(pedidoId);
            var user = await _userManager.GetUserAsync(User);

            if (pedido == null || pedido.EstudianteId != user.Id || pedido.Estado != "Pendiente")
            {
                TempData["ErrorMessage"] = "El pedido no es válido para pago.";
                return RedirectToAction("Index");
            }

            // *** NUEVA LÍNEA: Obtener la Tasa Oficial BCV ***
            var tasaHoy = await _exchangeRateService.GetTasaToday();

            // Si no hay tasa, usamos un valor por defecto (o mostramos error, mejor un valor de error)
            if (!tasaHoy.HasValue || tasaHoy.Value <= 0)
            {
                TempData["ErrorMessage"] = "No se pudo obtener la tasa de cambio oficial. Intente más tarde.";
                return RedirectToAction("Index");
            }


            // El ViewModel se prepara correctamente
            var viewModel = new PagoViewModel
            {
                PedidoId = pedidoId,
                Total = pedido.Total, // Este es el total en USD
                                      // *** NUEVA PROPIEDAD ***
                TasaBCV = tasaHoy.Value
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(PagoViewModel model)
        {
            // *** Se añade una validación explícita para el select MetodoPago ***
            if (string.IsNullOrEmpty(model.MetodoPago))
            {
                ModelState.AddModelError("MetodoPago", "Debe seleccionar un método de pago.");
            }

            // Si la validación falla (incluyendo la del select), se regresa a la vista
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Datos de pago inválidos. Por favor revise el formulario.";
                // Re-obtener el total del pedido si no está en el model para mostrarlo correctamente
                var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(model.PedidoId);
                if (pedido != null)
                {
                    model.Total = pedido.Total;
                }
                return View("Pagar", model);
            }

            // Lógica para guardar el archivo
            string urlComprobante = await GuardarComprobante(model.ComprobanteArchivo);

            // Aquí puedes añadir más validación si el archivo es obligatorio
            if (string.IsNullOrEmpty(urlComprobante))
            {
                TempData["ErrorMessage"] = "Es obligatorio adjuntar el comprobante de pago.";
                // Vuelve a cargar la vista con error
                return View("Pagar", model);
            }

            var pago = new Pago
            {
                MetodoPago = model.MetodoPago,
                ReferenciaPago = model.ReferenciaPago,
                UrlComprobante = urlComprobante
            };

            // Esto actualizará el estado del Pedido a "En Validación"
            await _compraService.RegistrarPagoAsync(model.PedidoId, pago);

            // Mensaje de éxito con la notificación de espera
            TempData["PagoRegistrado"] = true;
            return RedirectToAction("Index");
        }

        private async Task<string> GuardarComprobante(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            var fileName = Path.GetFileName(archivo.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes", fileName);

            // Asegurar que el directorio existe
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes"));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Retornar la ruta relativa para guardar en la base de datos
            return Path.Combine("/comprobantes", fileName).Replace("\\", "/");
        }

        // El resto de los métodos como Descargar se mantienen igual
        [Authorize]
        public async Task<IActionResult> Descargar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var hasPurchased = await _compraService.HasUserPurchasedProductAsync(user.Id, id);

            if (!hasPurchased)
            {
                return Forbid();
            }

            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null || string.IsNullOrEmpty(producto.UrlArchivoDigital))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", producto.UrlArchivoDigital.TrimStart('~', '/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            var contentType = "application/pdf";

            return File(fileBytes, contentType, fileName);
        }
    }
}