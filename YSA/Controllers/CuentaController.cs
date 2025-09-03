using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace YSA.Web.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;

        public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
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
    }
}
