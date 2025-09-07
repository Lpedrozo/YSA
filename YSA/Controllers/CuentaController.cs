using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Web.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YSA.Core.Services;

namespace YSA.Web.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IUsuarioService _usuarioService;

        public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ApplicationDbContext context, IUsuarioService usuarioService)
        {
            _userManager = userManager;
            _usuarioService = usuarioService;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Cuenta/Registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: /Cuenta/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var usuario = new Usuario
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                Nombre = viewModel.Nombre,
                Apellido = viewModel.Apellido,
                FechaCreacion = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(usuario, viewModel.Contrasena);

            if (resultado.Succeeded)
            {
                // Asigna el rol "Estudiante" por defecto
                await _userManager.AddToRoleAsync(usuario, "Estudiante");

                // Inicia sesión al usuario recién registrado
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        // GET: /Cuenta/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(viewModel.Email);
                if (usuario != null)
                {
                    var resultado = await _signInManager.PasswordSignInAsync(usuario, viewModel.Contrasena, viewModel.RecordarMe, lockoutOnFailure: false);

                    if (resultado.Succeeded)
                    {
                        // Inicia sesión exitosa, guardamos el estado de autenticación.
                        HttpContext.Session.SetString("IsSignedIn", "true");

                        // **Paso clave: Obtener y guardar el rol en la sesión**
                        var roles = await _userManager.GetRolesAsync(usuario);
                        if (roles.Any())
                        {
                            // Asume un solo rol para simplificar. Puedes ajustar esto si un usuario puede tener múltiples roles.
                            var rol = roles.First();
                            HttpContext.Session.SetString("UserRole", rol);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Remove("IsSignedIn");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> MiPerfil()
        {
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Aquí obtenemos los pedidos completados y sus cursos asociados.
                var pedidosCompletados = await _context.Pedidos
                    .Where(p => p.EstudianteId == usuario.Id && p.Estado == "Completado")
                    .Include(p => p.PedidoItems)
                        .ThenInclude(pi => pi.VentaItem)
                            .ThenInclude(vi => vi.Curso)
                    .ToListAsync();

                var cursosComprados = new List<CursoViewModel>();
                foreach (var pedido in pedidosCompletados)
                {
                    foreach (var item in pedido.PedidoItems)
                    {
                        // Mapear la entidad Curso al ViewModel de Curso
                        cursosComprados.Add(new CursoViewModel
                        {
                            Id = item.VentaItem.Curso.Id,
                            Titulo = item.VentaItem.Curso.Titulo,
                            DescripcionCorta = item.VentaItem.Curso.DescripcionCorta,
                            Precio = item.VentaItem.Curso.Precio,
                            UrlImagen = item.VentaItem.Curso.UrlImagen
                        });
                    }
                }

                var viewModel = new UserViewModel
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    CursosComprados = cursosComprados
                };

                return View("MiPerfil", viewModel);
            }

            // Si el usuario no está autenticado, redirigir al login
            return RedirectToAction("Login", "Cuenta");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(MiPerfil));
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Nombre = viewModel.Nombre;
            usuario.Apellido = viewModel.Apellido;
            var resultado = await _usuarioService.UpdateUsuarioAsync(usuario);

            if (resultado.Succeeded)
            {
                // Actualizar la sesión o cookies si es necesario
                await _signInManager.RefreshSignInAsync(usuario);
                // Si la actualización es exitosa, redirigir a la misma página para mostrar los cambios
                return RedirectToAction(nameof(MiPerfil));
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(MiPerfil));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Manejar errores de validación si es necesario, quizás redirigiendo con un modelo de error
                return RedirectToAction(nameof(MiPerfil));
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return NotFound();
            }

            var resultado = await _usuarioService.ChangePasswordAsync(usuario, viewModel.CurrentPassword, viewModel.NewPassword);

            if (resultado.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(usuario);
                // Si el cambio de contraseña es exitoso, puedes redirigir a la misma página con un mensaje de éxito
                return RedirectToAction(nameof(MiPerfil));
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(MiPerfil));
        }
    }
}
