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
using YSA.Data.Migrations;

namespace YSA.Web.Controllers
{
    public class PaqueteController : Controller
    {
        private readonly IPaqueteService _paqueteService;
        private readonly IEmailService _emailService;
        private readonly ICompraService _compraService;
        private readonly IPedidoService _pedidoService;
        private readonly INotificacionService _notificacionService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICursoService _cursoService;
        private readonly IProductoService _productoService;
        private readonly IEstudianteCursoService _estudianteCursoService;

        public PaqueteController(
            IEmailService emailService,
            IPaqueteService paqueteService,
            ICompraService compraService,
            UserManager<Usuario> userManager,
            IPedidoService pedidoService,
            IExchangeRateService exchangeRateService,
            INotificacionService notificacionService,
            ICursoService cursoService,
            IProductoService productoService,
            IEstudianteCursoService estudianteCursoService)
        {
            _paqueteService = paqueteService;
            _emailService = emailService;
            _compraService = compraService;
            _userManager = userManager;
            _pedidoService = pedidoService;
            _exchangeRateService = exchangeRateService;
            _notificacionService = notificacionService;
            _cursoService = cursoService;
            _productoService = productoService;
            _estudianteCursoService = estudianteCursoService;
        }

        // GET: /Paquete/Index
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            const int pageSize = 9;

            var todosLosPaquetes = await _paqueteService.ObtenerTodosConDetallesAsync();

            // Aplicar filtro de búsqueda
            var paquetesFiltrados = todosLosPaquetes.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                paquetesFiltrados = paquetesFiltrados.Where(p =>
                    p.Titulo.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.DescripcionCorta.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Paginación
            var paquetesOrdenados = paquetesFiltrados.OrderByDescending(p => p.Id).ToList();
            var totalPaquetes = paquetesOrdenados.Count();
            var totalPaginas = (int)Math.Ceiling(totalPaquetes / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPaginas && totalPaginas > 0) page = totalPaginas;

            var paquetesPaginados = paquetesOrdenados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Lógica de autenticación y estados de compra
            var paquetesCompradosIds = new List<int>();
            var paquetesEnValidacionIds = new List<int>();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    paquetesCompradosIds = (await _compraService.GetPurchasedPackageIdsAsync(user.Id)).ToList();
                    paquetesEnValidacionIds = (await _compraService.GetPackagesInValidationIdsAsync(user.Id)).ToList();
                }
            }

            var viewModel = new PaquetesIndexViewModel
            {
                Paquetes = paquetesPaginados,
                PaquetesCompradosIds = paquetesCompradosIds,
                PaquetesEnValidacionIds = paquetesEnValidacionIds,
                PaginaActual = page,
                TotalPaginas = totalPaginas,
                SearchString = searchString
            };

            return View(viewModel);
        }

        // GET: /Paquete/Detalle/{id}
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(id);
            if (paquete == null)
            {
                return NotFound();
            }

            bool tieneAcceso = false;
            bool estaEnValidacion = false;
            bool perfilCompleto = false;
            bool tienePedidoPendiente = false;
            int? pedidoPendienteId = null;
            var viewModel = new PaqueteDetalleViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    // Verificar si el usuario tiene perfil completo
                    perfilCompleto = UsuarioTienePerfilCompleto(usuario);

                    // Verificar si ya tiene acceso al paquete (ya lo compró)
                    tieneAcceso = await _compraService.HasUserPurchasedPackageAsync(usuario.Id, id);
                    viewModel.UsuarioNombre = usuario.Nombre;
                    viewModel.UsuarioApellido = usuario.Apellido;
                    if (!tieneAcceso)
                    {
                        // Verificar si tiene pedido pendiente
                        var pedidoPendiente = await _pedidoService.ObtenerPedidoPendientePorPaqueteAsync(usuario.Id, id);

                        if (pedidoPendiente != null)
                        {
                            tienePedidoPendiente = true;
                            pedidoPendienteId = pedidoPendiente.Id;

                            // Si el pedido está en estado "Pendiente" (sin pago registrado), el usuario puede continuar
                            // Si está en "Validando" (ya subió comprobante), está en proceso de validación
                            estaEnValidacion = pedidoPendiente.Estado == "Validando";

                            // Si está en "Pendiente", no mostramos "en validación", permitimos continuar
                        }
                    }
                }
            }

            // Calcular ahorro
            var precioTotalItems = (paquete.PaqueteCursos?.Sum(pc => pc.Curso?.Precio ?? 0) ?? 0) +
                                   (paquete.PaqueteProductos?.Sum(pp => pp.Producto?.Precio ?? 0) ?? 0);
            var ahorro = precioTotalItems - paquete.Precio;


            viewModel.Id = paquete.Id;
                viewModel.Titulo = paquete.Titulo;
                viewModel.DescripcionCorta = paquete.DescripcionCorta;
            viewModel.DescripcionLarga = paquete.DescripcionLarga;
                viewModel.Precio = paquete.Precio;
                viewModel.UrlImagen = paquete.UrlImagen;
                viewModel.PrecioTotalItems = precioTotalItems;
                viewModel.Ahorro = ahorro > 0 ? ahorro : 0;
            viewModel.FechaPublicacion = paquete.FechaPublicacion;
                viewModel.EsDestacado = paquete.EsDestacado;
                viewModel.EsRecomendado = paquete.EsRecomendado;
                viewModel.Cursos = paquete.PaqueteCursos?.Select(pc => new PaqueteCursoViewModel
                {
                    Id = pc.Curso.Id,
                    Titulo = pc.Curso.Titulo,
                    DescripcionCorta = pc.Curso.DescripcionCorta,
                    UrlImagen = pc.Curso.UrlImagen,
                    Precio = pc.Curso.Precio
                }).ToList() ?? new List<PaqueteCursoViewModel>();
                viewModel.Productos = paquete.PaqueteProductos?.Select(pp => new PaqueteProductoViewModel
                {
                    Id = pp.Producto.Id,
                    Titulo = pp.Producto.Titulo,
                    DescripcionCorta = pp.Producto.DescripcionCorta,
                    UrlImagen = pp.Producto.UrlImagen,
                    Precio = pp.Producto.Precio,
                    TipoProducto = pp.Producto.TipoProducto
                }).ToList() ?? new List<PaqueteProductoViewModel>();
                viewModel.TieneAcceso = tieneAcceso;
                viewModel.EstaEnValidacion = estaEnValidacion;
                viewModel.PerfilCompleto = perfilCompleto;
               viewModel.TienePedidoPendiente = tienePedidoPendiente;
                viewModel.UsuarioLogueado = User.Identity.IsAuthenticated;
                viewModel.PedidoPendienteId = pedidoPendienteId;

            return View(viewModel);
        }
        // POST: /Paquete/Comprar
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Comprar(int paqueteId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no autenticado.";
                return RedirectToAction("Index");
            }

            // Verificar si el usuario tiene perfil completo
            if (!UsuarioTienePerfilCompleto(user))
            {
                TempData["RedirectToProfile"] = true;
                TempData["Message"] = "Completa tus datos personales antes de comprar un paquete.";
                return RedirectToAction("Detalle", new { id = paqueteId });
            }

            // Verificar si ya compró el paquete
            var yaComprado = await _compraService.HasUserPurchasedPackageAsync(user.Id, paqueteId);
            if (yaComprado)
            {
                TempData["ErrorMessage"] = "Ya has adquirido este paquete.";
                return RedirectToAction("Detalle", new { id = paqueteId });
            }

            // Verificar si tiene un pedido pendiente (estado "Pendiente" - sin pago)
            var pedidoPendiente = await _pedidoService.ObtenerPedidoPendientePorPaqueteAsync(user.Id, paqueteId);

            // Si existe un pedido en estado "Pendiente", lo reutilizamos
            if (pedidoPendiente != null && pedidoPendiente.Estado == "Pendiente")
            {
                return RedirectToAction("Pagar", new { pedidoId = pedidoPendiente.Id });
            }

            // Si existe un pedido en estado "Validando", mostrar mensaje
            if (pedidoPendiente != null && pedidoPendiente.Estado == "Validando")
            {
                TempData["InfoMessage"] = "Ya tienes un pedido pendiente de validación para este paquete.";
                return RedirectToAction("Detalle", new { id = paqueteId });
            }

            // Si no existe pedido pendiente, crear uno nuevo
            var pedido = await _compraService.IniciarCompraPaqueteAsync(paqueteId, user.Id);

            if (pedido == null)
            {
                TempData["ErrorMessage"] = "El paquete no existe o la compra no pudo iniciarse.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Pagar", new { pedidoId = pedido.Id });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Pagar(int pedidoId)
        {
            // Usar el método que carga las relaciones, no el básico
            var pedido = await _pedidoService.ObtenerPedidoConItemsYVentaItemsAsync(pedidoId);
            var user = await _userManager.GetUserAsync(User);

            if (pedido == null || pedido.EstudianteId != user.Id || pedido.Estado != "Pendiente")
            {
                TempData["ErrorMessage"] = "El pedido no es válido para pago.";
                return RedirectToAction("Index");
            }

            // Verificar si el usuario tiene perfil completo
            if (!UsuarioTienePerfilCompleto(user))
            {
                TempData["RedirectToProfile"] = true;
                TempData["Message"] = "Completa tus datos personales antes de realizar el pago.";
                return RedirectToAction("CompletarPerfil", "Cuenta", new { returnUrl = Url.Action("Pagar", new { pedidoId }) });
            }

            var tasaHoy = await _exchangeRateService.GetTasaToday();

            if (!tasaHoy.HasValue || tasaHoy.Value <= 0)
            {
                TempData["ErrorMessage"] = "No se pudo obtener la tasa de cambio oficial. Intente más tarde.";
                return RedirectToAction("Index");
            }

            // Obtener el paquete para mostrar detalles - AHORA CON VERIFICACIÓN DE NULL
            int? paqueteId = null;
            Paquete paquete = null;

            // Verificar que PedidoItems no sea null y tenga elementos
            if (pedido.PedidoItems != null && pedido.PedidoItems.Any())
            {
                var primerItem = pedido.PedidoItems.FirstOrDefault();
                if (primerItem?.VentaItem != null)
                {
                    paqueteId = primerItem.VentaItem.PaqueteId;
                    if (paqueteId.HasValue)
                    {
                        paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(paqueteId.Value);
                    }
                }
            }

            var viewModel = new PagoPaqueteViewModel
            {
                PedidoId = pedidoId,
                Total = pedido.Total,
                TasaBCV = tasaHoy.Value,
                PaqueteTitulo = paquete?.Titulo ?? "Paquete",
                PaqueteImagen = paquete?.UrlImagen,
                CantidadItems = (paquete?.PaqueteCursos?.Count ?? 0) + (paquete?.PaqueteProductos?.Count ?? 0)
            };

            return View(viewModel);
        }
        // POST: /Paquete/ProcesarPago
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(PagoPaqueteViewModel model)
        {
            if (string.IsNullOrEmpty(model.MetodoPago))
            {
                ModelState.AddModelError("MetodoPago", "Debe seleccionar un método de pago.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Datos de pago inválidos. Por favor revise el formulario.";
                var pedido = await _pedidoService.ObtenerPedidoPorIdAsync(model.PedidoId);
                if (pedido != null)
                {
                    model.Total = pedido.Total;
                }
                return View("Pagar", model);
            }

            // Guardar comprobante
            string urlComprobante = await GuardarComprobante(model.ComprobanteArchivo);

            if (string.IsNullOrEmpty(urlComprobante))
            {
                TempData["ErrorMessage"] = "Es obligatorio adjuntar el comprobante de pago.";
                return View("Pagar", model);
            }
            var pedidoExistente = await _pedidoService.ObtenerPedidoPorIdAsync(model.PedidoId);
            if (pedidoExistente == null)
            {
                TempData["ErrorMessage"] = "El pedido no existe. Por favor, intenta nuevamente.";
                return RedirectToAction("Index");
            }
            var pago = new Pago
            {
                MetodoPago = model.MetodoPago,
                ReferenciaPago = model.ReferenciaPago,
                UrlComprobante = urlComprobante,
                PedidoId = model.PedidoId,
                FechaPago = DateTime.Now
            };
            await _pedidoService.RegistrarPagoAsync(pago);

            // Actualizar el estado del pedido a "Validando"
            await _pedidoService.ActualizarEstadoPedidoAsync(model.PedidoId, "Validando");
            var user = await _userManager.GetUserAsync(User);
            try
            {
                await _emailService.EnviarNotificacionAdminPagoPendienteAsync(
                    $"{user?.Nombre} {user?.Apellido}",
                user?.Email ?? "",
                    "Paquete",
                    "Paquete",
                    model.Total,
                    model.PedidoId,
                    urlComprobante
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo al admin: {ex.Message}");
            }
            var pedidoCompletado = await _pedidoService.ObtenerPedidoPorIdAsync(model.PedidoId);
            if (pedidoCompletado != null)
            {
                // Enviar notificación a los administradores
                await _notificacionService.CrearNotificacionNuevoPedidoAsync(
                    model.PedidoId,
                    pedidoCompletado.Total,
                    "Paquete"
                );

                if (user != null)
                {
                    // Notificar al usuario que su pago está en validación
                    await _notificacionService.CrearNotificacionPagoPendienteAsync(
                        user.Id,
                        model.PedidoId,
                        pedidoCompletado.Total
                    );
                }
            }

            TempData["PagoRegistrado"] = true;
            return RedirectToAction("Index");
        }

        // GET: /Paquete/MisPaquetes
        [Authorize]
        public async Task<IActionResult> MisPaquetes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var paquetesComprados = await _compraService.GetPurchasedPackagesWithDetailsAsync(user.Id);

            var viewModel = paquetesComprados.Select(p => new PaqueteListaViewModel
            {
                Id = p.Id,
                Titulo = p.Titulo,
                DescripcionCorta = p.DescripcionCorta,
                UrlImagen = p.UrlImagen,
                Precio = p.Precio,
                CantidadItems = (p.PaqueteCursos?.Count ?? 0) + (p.PaqueteProductos?.Count ?? 0)
            }).ToList();

            return View(viewModel);
        }

        // GET: /Paquete/VerPaquete/{id} (para usuarios que ya lo compraron)
        [Authorize]
        public async Task<IActionResult> VerPaquete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            var tieneAcceso = await _compraService.HasUserPurchasedPackageAsync(user.Id, id);
            if (!tieneAcceso)
            {
                return RedirectToAction("Detalle", new { id });
            }

            var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(id);
            if (paquete == null)
            {
                return NotFound();
            }

            // Verificar qué cursos ya tiene el estudiante
            var cursosIdsComprados = await _estudianteCursoService.GetEstudianteCursoIdsAsync(user.Id);

            var viewModel = new PaqueteVerViewModel
            {
                Id = paquete.Id,
                Titulo = paquete.Titulo,
                DescripcionLarga = paquete.DescripcionLarga,
                UrlImagen = paquete.UrlImagen,
                Cursos = paquete.PaqueteCursos?.Select(pc => new PaqueteCursoConAccesoViewModel
                {
                    Id = pc.Curso.Id,
                    Titulo = pc.Curso.Titulo,
                    DescripcionCorta = pc.Curso.DescripcionCorta,
                    UrlImagen = pc.Curso.UrlImagen,
                    TieneAcceso = cursosIdsComprados.Contains(pc.Curso.Id),
                    UrlAcceso = Url.Action("VerCurso", "Curso", new { id = pc.Curso.Id })
                }).ToList() ?? new List<PaqueteCursoConAccesoViewModel>(),
                Productos = paquete.PaqueteProductos?.Select(pp => new PaqueteProductoConAccesoViewModel
                {
                    Id = pp.Producto.Id,
                    Titulo = pp.Producto.Titulo,
                    DescripcionCorta = pp.Producto.DescripcionCorta,
                    UrlImagen = pp.Producto.UrlImagen,
                    TipoProducto = pp.Producto.TipoProducto,
                    UrlDescarga = Url.Action("Descargar", "Producto", new { id = pp.Producto.Id })
                }).ToList() ?? new List<PaqueteProductoConAccesoViewModel>()
            };

            return View(viewModel);
        }

        // Método auxiliar para verificar si el usuario tiene el perfil completo
        private bool UsuarioTienePerfilCompleto(Usuario usuario)
        {
            return !string.IsNullOrEmpty(usuario.Cedula) &&
                   !string.IsNullOrEmpty(usuario.WhatsApp) &&
                   usuario.FechaNacimiento.HasValue;
        }

        // Método auxiliar para guardar comprobante
        private async Task<string> GuardarComprobante(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/comprobantes/{fileName}";
        }
    }
}