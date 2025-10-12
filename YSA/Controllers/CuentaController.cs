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
using Microsoft.AspNetCore.Authorization;

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
                FechaCreacion = DateTime.UtcNow,
                UrlImagen = "/FotoPerfil/usuariopredeterminada.jpg"
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

        // Modificación del método ChangePassword/UpdatePassword en CuentaController (o IUsuarioService)

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

            // Paso Clave: Verificar si el usuario ya tiene una contraseña local
            var hasPassword = await _userManager.HasPasswordAsync(usuario);

            IdentityResult resultado;

            if (hasPassword)
            {
                // Caso 1: El usuario tiene contraseña local (se registró tradicionalmente o ya la había configurado).
                // Se requiere la contraseña actual para seguridad.
                if (string.IsNullOrEmpty(viewModel.CurrentPassword))
                {
                    ModelState.AddModelError(string.Empty, "Debes proporcionar tu contraseña actual para cambiarla.");
                    return RedirectToAction(nameof(MiPerfil));
                }

                // Usamos ChangePasswordAsync, que valida la contraseña actual
                resultado = await _userManager.ChangePasswordAsync(
                    usuario,
                    viewModel.CurrentPassword,
                    viewModel.NewPassword);
            }
            else
            {
                // Caso 2: El usuario NO tiene contraseña local (se registró con Google/Facebook, etc.).
                // NO se requiere CurrentPassword. Usamos AddPasswordAsync para crearla.

                // El campo CurrentPassword de la vista será ignorado.
                resultado = await _userManager.AddPasswordAsync(usuario, viewModel.NewPassword);
            }

            // El resto de la lógica de manejo de resultados es la misma

            if (resultado.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(usuario);
                TempData["SuccessMessage"] = hasPassword
                    ? "Contraseña cambiada con éxito."
                    : "Contraseña local establecida con éxito.";
            }
            else
            {
                // Manejo de errores
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["ErrorMessage"] = "Error al procesar la contraseña.";
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
        // POST: /Cuenta/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // 1. Define la URL a la que Google debe devolver la respuesta (callback)
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Cuenta", new { returnUrl });

            // 2. Configura las propiedades que Identity necesita para el desafío (challenge)
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // 3. Ejecuta el desafío. Esto redirige al usuario a la página de inicio de sesión de Google.
            return new ChallengeResult(provider, properties);
        }
        // GET: /Cuenta/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["Error"] = $"Error del proveedor: {remoteError}";
                return RedirectToAction(nameof(Registro));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Error cargando la información de inicio de sesión externa.";
                return RedirectToAction(nameof(Registro));
            }

            // 1. Intentar iniciar sesión con el proveedor externo
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // CASO 1: Usuario Existente (ya tiene un vínculo con Google)
                // O: Se le permitió ingresar sin 2FA si ya lo tenía configurado.
                return LocalRedirect(returnUrl ?? "/Home/Index");
            }

            // Si no fue exitoso y NO es porque la cuenta está bloqueada o requiere 2FA.
            // Esto cubre los casos donde el usuario de Google es NUEVO y necesita REGISTRO.
            if (result.IsLockedOut)
            {
                // Si el usuario está bloqueado por muchos intentos fallidos (aunque es raro con SSO)
                TempData["Error"] = "La cuenta del usuario está bloqueada.";
                return RedirectToAction(nameof(Login)); // O a la vista que maneje el bloqueo
            }

            if (result.IsNotAllowed)
            {
                // La cuenta no está permitida (ej. no EmailConfirmed, aunque Google ya lo confirma)
                TempData["Error"] = "La cuenta no está permitida para iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }

            // 2. Nuevo Usuario: Creación de la cuenta (REEMPLAZA A requiresRegistration)
            if (result.Succeeded == false)
            {
                // 2.1 Extraer la información del claim de Google
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                // **Comprobación adicional:** Verificar si el email ya existe en el sistema (posible usuario registrado manualmente).
                var existingUser = await _userManager.FindByEmailAsync(email);

                if (existingUser != null)
                {
                    // CASO ESPECIAL: El usuario ya existe en tu DB (por registro manual) pero no tiene el login de Google asociado.
                    // Simplemente asocia el login de Google a la cuenta existente.
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl ?? "/Home/Index");
                    }
                    // Si falla la asociación, puede ser que el login ya esté asignado a otra cuenta.
                    TempData["Error"] = "El correo electrónico ya está registrado. Error al asociar la cuenta de Google. Contacte soporte.";
                    return RedirectToAction(nameof(Registro));
                }

                // Si el email NO existe, procedemos a la creación de la cuenta
                var nombre = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var apellido = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var pictureUrl = info.Principal.FindFirstValue("picture");

                // 2.2 Crear la entidad de usuario
                var usuario = new Usuario
                {
                    UserName = email,
                    Email = email,
                    Nombre = nombre ?? "",
                    Apellido = apellido ?? "",
                    FechaCreacion = DateTime.UtcNow,
                    EmailConfirmed = true,
                    UrlImagen = pictureUrl ?? "/FotoPerfil/usuariopredeterminada.jpg"
                };

                var createResult = await _userManager.CreateAsync(usuario);

                if (createResult.Succeeded)
                {
                    // 2.3 Asociar el inicio de sesión externo (Google) con el nuevo usuario
                    createResult = await _userManager.AddLoginAsync(usuario, info);

                    if (createResult.Succeeded)
                    {
                        // 2.4 Asignar el rol por defecto (Estudiante)
                        // Asegúrate de que este rol exista en tu base de datos
                        await _userManager.AddToRoleAsync(usuario, "Estudiante");

                        // 2.5 Iniciar sesión
                        await _signInManager.SignInAsync(usuario, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl ?? "/Home/Index");
                    }
                }

                // Manejo de errores de creación
                TempData["Error"] = createResult.Errors.Any() ? createResult.Errors.First().Description : "Error desconocido al crear la cuenta con Google.";
                return RedirectToAction(nameof(Registro));
            }

            // Si el flujo no cae en Succeeded ni en las demás condiciones, devuelve un error genérico.
            TempData["Error"] = "Fallo al procesar el inicio de sesión externo (Resultado desconocido).";
            return RedirectToAction(nameof(Registro));
        }
    }
}
