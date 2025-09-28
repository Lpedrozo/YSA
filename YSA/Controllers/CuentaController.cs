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
using System.Data;
using Microsoft.AspNetCore.Hosting;

namespace YSA.Web.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IUsuarioService _usuarioService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ApplicationDbContext context, IUsuarioService usuarioService, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _usuarioService = usuarioService;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

                // Obtener todos los PedidoItems completados para el usuario, incluyendo VentaItem y sus relaciones de Curso y Producto.
                var pedidoItemsCompletados = await _context.PedidoItems
                    .Where(pi => pi.Pedido.EstudianteId == usuario.Id && pi.Pedido.Estado == "Completado")
                    .Include(pi => pi.VentaItem)
                        .ThenInclude(vi => vi.Curso)
                    .Include(pi => pi.VentaItem)
                        .ThenInclude(vi => vi.Producto)
                    .ToListAsync();

                var cursosComprados = new List<CursoViewModel>();
                var productosComprados = new List<ProductoViewModel>();

                foreach (var item in pedidoItemsCompletados)
                {
                    if (item.VentaItem.Curso != null)
                    {
                        cursosComprados.Add(new CursoViewModel
                        {
                            Id = item.VentaItem.Curso.Id,
                            Titulo = item.VentaItem.Curso.Titulo,
                            DescripcionCorta = item.VentaItem.Curso.DescripcionCorta,
                            Precio = item.VentaItem.Curso.Precio,
                            UrlImagen = item.VentaItem.Curso.UrlImagen
                        });
                    }
                    else if (item.VentaItem.Producto != null)
                    {
                        productosComprados.Add(new ProductoViewModel
                        {
                            Id = item.VentaItem.Producto.Id,
                            Titulo = item.VentaItem.Producto.Titulo,
                            DescripcionCorta = item.VentaItem.Producto.DescripcionCorta,
                            Precio = item.VentaItem.Producto.Precio,
                            UrlImagen = item.VentaItem.Producto.UrlImagen
                        });
                    }
                }

                var viewModel = new UserViewModel
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    CursosComprados = cursosComprados,
                    ProductosComprados = productosComprados,
                    UrlImagen = usuario.UrlImagen
                };

                return View("MiPerfil", viewModel);
            }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Por favor, selecciona una imagen para subir.";
                return RedirectToAction(nameof(MiPerfil));
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return NotFound();
            }

            try
            {
                string userIdString = usuario.Id.ToString();

                // 1. Definir la ruta de la carpeta del usuario.
                // Esta es la parte clave para crear la estructura que quieres: wwwroot/FotoPerfil/UsuarioId
                string userFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPerfil", userIdString);

                // 2. Crear la carpeta si no existe.
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                }

                // 3. Eliminar la imagen anterior si existe.
                // El nombre de la imagen anterior se obtiene del final de la URL guardada en la base de datos.
                if (!string.IsNullOrEmpty(usuario.UrlImagen))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, usuario.UrlImagen.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // 4. Generar un nombre único para la nueva imagen y la ruta completa del archivo.
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(userFolder, uniqueFileName);

                // 5. Guardar la nueva imagen en la carpeta del usuario.
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 6. Actualizar la propiedad UrlImagen con la nueva ruta relativa.
                // Esta es la URL que se guardará en la base de datos.
                usuario.UrlImagen = Path.Combine("/FotoPerfil", userIdString, uniqueFileName).Replace("\\", "/");

                // 7. Persistir los cambios en la base de datos.
                var resultado = await _userManager.UpdateAsync(usuario);

                if (resultado.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(usuario);
                    TempData["SuccessMessage"] = "Foto de perfil actualizada con éxito.";
                }
                else
                {
                    // Si falla la actualización, eliminar la imagen recién subida.
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    TempData["ErrorMessage"] = "Error al actualizar la foto de perfil.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al subir la imagen. Inténtalo de nuevo.";
            }

            return RedirectToAction(nameof(MiPerfil));
        }

    }
}
