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
                // Asignar rol según el tipo de cuenta seleccionado
                if (viewModel.TipoCuenta == "Artista")
                {
                    // Asignar rol Artista
                    await _userManager.AddToRoleAsync(usuario, "Artista");

                    // Crear registro en tabla Artistas
                    var artista = new Artista
                    {
                        UsuarioId = usuario.Id,
                        NombreArtistico = viewModel.NombreArtistico ?? usuario.Nombre,
                        Biografia = viewModel.Biografia ?? "",
                        EstiloPrincipal = viewModel.EstiloPrincipal ?? "",
                        EsAcademia = false,                     // Es artista externo
                        EstadoAprobacion = "PendienteAprobacion", // Pendiente de aprobación
                        FechaSolicitud = DateTime.UtcNow
                    };

                    _context.Artistas.Add(artista);
                    await _context.SaveChangesAsync();

                    await _userManager.AddToRoleAsync(usuario, "Estudiante");

                    TempData["SuccessMessage"] = "Tu cuenta de artista ha sido creada. Está pendiente de aprobación por el administrador. Recibirás una notificación cuando sea activada.";
                }
                else
                {
                    // Asignar rol Estudiante
                    await _userManager.AddToRoleAsync(usuario, "Estudiante");
                    TempData["SuccessMessage"] = "¡Cuenta creada exitosamente!";
                }

                // Iniciar sesión
                await _signInManager.SignInAsync(usuario, isPersistent: false);

                // Redirigir según el tipo de cuenta
                if (viewModel.TipoCuenta == "Artista")
                {
                    return RedirectToAction("Dashboard", "Artista"); // Pendiente crear
                }

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
        // En CuentaController.cs - Agregar después del método Registro()

        // POST: /Cuenta/ValidarDatosBasicos
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarDatosBasicos([FromBody] ValidarDatosBasicosRequest request)
        {
            try
            {
                // Validar si el email ya existe
                var existingUser = await _userManager.FindByEmailAsync(request.Email);

                if (existingUser != null)
                {
                    // Verificar si el usuario ya tiene roles asignados
                    var roles = await _userManager.GetRolesAsync(existingUser);

                    // Si el usuario tiene roles, no puede registrarse de nuevo
                    if (roles.Any())
                    {
                        return Json(new
                        {
                            existe = true,
                            tieneRoles = true,
                            mensaje = "Este correo ya está registrado. Por favor inicia sesión."
                        });
                    }

                    // Usuario existe pero no tiene roles (registro incompleto)
                    // Guardamos el ID en una variable de sesión temporal
                    HttpContext.Session.SetInt32("UsuarioTempId", existingUser.Id);

                    return Json(new
                    {
                        existe = true,
                        tieneRoles = false,
                        mensaje = "Continuemos con la configuración de tu cuenta.",
                        datos = new
                        {
                            nombre = existingUser.Nombre,
                            apellido = existingUser.Apellido,
                            email = existingUser.Email
                        }
                    });
                }

                // Usuario no existe, guardar datos temporalmente en sesión
                var datosTemp = new DatosBasicosTemp
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Email = request.Email,
                    Contrasena = request.Contrasena
                };

                HttpContext.Session.SetString("DatosBasicosTemp", System.Text.Json.JsonSerializer.Serialize(datosTemp));

                return Json(new { existe = false, mensaje = "Datos válidos, puedes continuar." });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = "Error al validar los datos: " + ex.Message });
            }
        }

        // POST: /Cuenta/AsignarRol
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AsignarRol([FromBody] AsignarRolRequest request)
        {
            try
            {
                Usuario usuario = null;
                bool esUsuarioExistente = false;

                // Verificar si hay un usuario temporal en sesión (existente sin rol)
                var usuarioTempId = HttpContext.Session.GetInt32("UsuarioTempId");

                if (usuarioTempId.HasValue)
                {
                    usuario = await _userManager.FindByIdAsync(usuarioTempId.Value.ToString());
                    if (usuario != null)
                    {
                        esUsuarioExistente = true;
                    }
                }

                // Si no hay usuario temporal, crear uno nuevo con los datos guardados
                if (usuario == null)
                {
                    var datosBasicosJson = HttpContext.Session.GetString("DatosBasicosTemp");
                    if (string.IsNullOrEmpty(datosBasicosJson))
                    {
                        return Json(new { success = false, mensaje = "No se encontraron datos básicos. Por favor reinicia el proceso." });
                    }

                    var datosBasicos = System.Text.Json.JsonSerializer.Deserialize<DatosBasicosTemp>(datosBasicosJson);

                    usuario = new Usuario
                    {
                        UserName = datosBasicos.Email,
                        Email = datosBasicos.Email,
                        Nombre = datosBasicos.Nombre,
                        Apellido = datosBasicos.Apellido,
                        FechaCreacion = DateTime.UtcNow,
                        UrlImagen = "/FotoPerfil/usuariopredeterminada.jpg"
                    };

                    var resultado = await _userManager.CreateAsync(usuario, datosBasicos.Contrasena);

                    if (!resultado.Succeeded)
                    {
                        return Json(new { success = false, mensaje = "Error al crear el usuario: " + string.Join(", ", resultado.Errors.Select(e => e.Description)) });
                    }
                }

                // Asignar rol y datos adicionales según el tipo
                if (request.TipoCuenta == "Estudiante")
                {
                    await _userManager.AddToRoleAsync(usuario, "Estudiante");

                    // Limpiar sesión temporal
                    HttpContext.Session.Remove("UsuarioTempId");
                    HttpContext.Session.Remove("DatosBasicosTemp");

                    // Iniciar sesión
                    await _signInManager.SignInAsync(usuario, isPersistent: false);

                    return Json(new
                    {
                        success = true,
                        mensaje = "Cuenta de estudiante creada exitosamente.",
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }
                else if (request.TipoCuenta == "Artista")
                {
                    // Asignar rol Artista
                    await _userManager.AddToRoleAsync(usuario, "Artista");

                    // También asignar rol Estudiante para que pueda comprar cursos
                    await _userManager.AddToRoleAsync(usuario, "Estudiante");

                    // Crear registro en tabla Artistas
                    var artista = new Artista
                    {
                        UsuarioId = usuario.Id,
                        NombreArtistico = request.NombreArtistico ?? usuario.Nombre,
                        Biografia = request.Biografia ?? "",
                        EstiloPrincipal = request.EstiloPrincipal ?? "",
                        EsAcademia = false,
                        EstadoAprobacion = "En revisión",
                        FechaSolicitud = DateTime.UtcNow,
                        MotivoRechazo = "En revisión"
                    };

                    _context.Artistas.Add(artista);
                    await _context.SaveChangesAsync();

                    // Limpiar sesión temporal
                    HttpContext.Session.Remove("UsuarioTempId");
                    HttpContext.Session.Remove("DatosBasicosTemp");

                    // IMPORTANTE: Iniciar sesión con un claim personalizado que indique el rol activo
                    await SignInWithSpecificRole(usuario, "Artista");

                    return Json(new
                    {
                        success = true,
                        mensaje = "Tu cuenta de artista ha sido creada. Está pendiente de aprobación por el administrador.",
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }
                return Json(new { success = false, mensaje = "Tipo de cuenta no válido." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al procesar la solicitud: " + ex.Message });
            }
        }
        // Método auxiliar para iniciar sesión con un rol específico
        private async Task SignInWithSpecificRole(Usuario usuario, string rolActivo)
        {
            // Obtener todos los roles del usuario
            var roles = await _userManager.GetRolesAsync(usuario);

            // Crear claims personalizados
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.UserName),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("NombreCompleto", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim("RolActivo", rolActivo)
            };

            // Agregar también el claim de rol tradicional (opcional, para compatibilidad)
            claims.Add(new Claim(ClaimTypes.Role, rolActivo));

            // Crear identidad y principal
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Iniciar sesión sin usar el SignInManager tradicional
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            // Guardar el rol activo en sesión para fácil acceso
            HttpContext.Session.SetString("RolActivo", rolActivo);
            HttpContext.Session.SetString("UserRole", rolActivo);
        }
        // POST: /Cuenta/ValidarLogin
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarLogin([FromBody] ValidarLoginRequest request)
        {
            try
            {
                var usuario = await _userManager.FindByEmailAsync(request.Email);

                if (usuario == null)
                {
                    return Json(new { success = false, mensaje = "Credenciales inválidas" });
                }

                var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, request.Contrasena, false);

                if (!resultado.Succeeded)
                {
                    return Json(new { success = false, mensaje = "Credenciales inválidas" });
                }

                var roles = await _userManager.GetRolesAsync(usuario);

                // CASO 1: Usuario con múltiples roles
                if (roles.Count > 1)
                {
                    // Guardar usuario en sesión temporal para login con rol específico
                    HttpContext.Session.SetInt32("LoginTempUserId", usuario.Id);
                    HttpContext.Session.SetString("LoginTempPassword", request.Contrasena);
                    HttpContext.Session.SetString("LoginTempRemember", request.RecordarMe.ToString());

                    return Json(new
                    {
                        success = true,
                        multiplesRoles = true,
                        roles = roles
                    });
                }

                // CASO 2: Usuario con un solo rol
                if (roles.Count == 1)
                {
                    var rolUnico = roles.First();

                    // Iniciar sesión directamente con el rol único
                    await SignInWithSpecificRole(usuario, rolUnico);

                    return Json(new
                    {
                        success = true,
                        multiplesRoles = false,
                        rolUnico = rolUnico,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }

                // CASO 3: Usuario sin roles (caso borde - debería tener al menos uno)
                // Asignar rol por defecto "Estudiante"
                await _userManager.AddToRoleAsync(usuario, "Estudiante");
                await SignInWithSpecificRole(usuario, "Estudiante");

                return Json(new
                {
                    success = true,
                    multiplesRoles = false,
                    rolUnico = "Estudiante",
                    redirectUrl = Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al validar: " + ex.Message });
            }
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginConRolRequest request)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("LoginTempUserId");

                if (!usuarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión expirada. Por favor intenta de nuevo." });
                }

                var usuario = await _userManager.FindByIdAsync(usuarioId.Value.ToString());

                if (usuario == null)
                {
                    return Json(new { success = false, mensaje = "Usuario no encontrado" });
                }

                // Verificar que el usuario tenga el rol seleccionado
                var roles = await _userManager.GetRolesAsync(usuario);
                if (!roles.Contains(request.RolSeleccionado))
                {
                    return Json(new { success = false, mensaje = "El rol seleccionado no está disponible para este usuario." });
                }

                // Iniciar sesión con el rol seleccionado
                await SignInWithSpecificRole(usuario, request.RolSeleccionado);

                // Limpiar sesión temporal
                HttpContext.Session.Remove("LoginTempUserId");
                HttpContext.Session.Remove("LoginTempPassword");
                HttpContext.Session.Remove("LoginTempRemember");

                return Json(new
                {
                    success = true,
                    mensaje = $"Bienvenido {usuario.Nombre}",
                    redirectUrl = request.ReturnUrl ?? Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = "Error al iniciar sesión: " + ex.Message });
            }
        }
        public class ValidarDatosBasicosRequest
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Contrasena { get; set; }
            public string ConfirmarContrasena { get; set; }
            public bool AceptaTerminos { get; set; }
        }

        public class AsignarRolRequest
        {
            public string TipoCuenta { get; set; }
            public string NombreArtistico { get; set; }
            public string EstiloPrincipal { get; set; }
            public string Biografia { get; set; }
        }
        public class ValidarLoginRequest
        {
            public string Email { get; set; }
            public string Contrasena { get; set; }
            public bool RecordarMe { get; set; }
        }

        public class LoginConRolRequest
        {
            public string Email { get; set; }
            public string Contrasena { get; set; }
            public bool RecordarMe { get; set; }
            public string RolSeleccionado { get; set; }
            public string ReturnUrl { get; set; }
        }
        public class DatosBasicosTemp
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Contrasena { get; set; }
        }
    }
}
