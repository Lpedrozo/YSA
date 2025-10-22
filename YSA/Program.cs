using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Core.Interfaces;
using YSA.Data.Repositories;
using YSA.Core.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Authentication; // <-- Esta es la clave

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// =======================================================
// INICIO: CONFIGURACIÓN GLOBAL DE CULTURA
// =======================================================
var cultureInfo = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new List<CultureInfo> { cultureInfo };
    options.SupportedUICultures = new List<CultureInfo> { cultureInfo };
    options.RequestCultureProviders.Clear();
});
// =======================================================
// FIN: CONFIGURACIÓN GLOBAL DE CULTURA
// =======================================================


// Configuración de la conexión a la base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(connectionString));

// Configuración de la sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuración de Identity
builder.Services.AddIdentity<Usuario, Rol>()
   .AddEntityFrameworkStores<ApplicationDbContext>()
   .AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("profile");
        options.ClaimActions.MapJsonKey("picture", "picture", "url");
    });

// Agrega los servicios de MVC (Controladores y Vistas)
builder.Services.AddControllersWithViews();

// **Paso clave: Inyección de Dependencias de tus Repositorios y Servicios**
// (Dejo el resto de tus registros aquí. Asumo que IRecursoActividadService estaba duplicado y lo eliminé.)
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddScoped<ILeccionRepository, LeccionRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IVentaItemRepository, VentaItemRepository>();

builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<ILeccionService, LeccionService>();
builder.Services.AddScoped<IVentaItemService, VentaItemService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEstudianteCursoRepository, EstudianteCursoRepository>();
builder.Services.AddScoped<IEstudianteCursoService, EstudianteCursoService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IArtistaService, ArtistaService>();
builder.Services.AddScoped<IArtistaRepository, ArtistaRepository>();
builder.Services.AddScoped<IProgresoLeccionRepository, ProgresoLeccionRepository>();
builder.Services.AddScoped<IProgresoLeccionService, ProgresoLeccionService>();
builder.Services.AddScoped<IArtistaFotoRepository, ArtistaFotoRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IEventoService, EventoService>();
// El ProductoRepository ya estaba arriba
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IRecursoActividadRepository, RecursoActividadRepository>();
builder.Services.AddScoped<IRecursoActividadService, RecursoActividadService>();
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
// **Registro de Tasa de Cambio**
builder.Services.AddScoped<ITasaBCVRepository, TasaBCVRepository>();

// 1. Configurar HttpClient y IExchangeRateService
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    // Cargar la URL base desde appsettings
    client.BaseAddress = new Uri(configuration["ExchangeRateApi:BaseUrl"]!);

    // Opcional: Configurar un timeout para evitar esperas infinitas
    client.Timeout = TimeSpan.FromSeconds(15);
});

// 2. Registrar el Worker Service para el guardado diario
builder.Services.AddHostedService<BcvRateWorker>();

var app = builder.Build();

// =====================================================================
// INICIO DE LA CORRECCIÓN DEL ERROR: InvalidOperationException
// =====================================================================

// La excepción ocurre porque la primera vez que se resuelve IExchangeRateService 
// (ya sea en el Worker o aquí), la configuración de HttpClientFactory puede
// intentar agregar algo al contenedor de DI. 
// Para evitar que esto suceda *dentro* del worker (donde es de solo lectura),
// forzamos su resolución e inicialización aquí, mientras es seguro.
try
{
    // Creamos un scope (necesario si ExchangeRateService fuera Scoped,
    // pero aunque es Scoped por AddHttpClient, es mejor tratarlo como tal).
    using (var scope = app.Services.CreateScope())
    {
        // Resolvemos el servicio. Esto dispara cualquier inicialización diferida.
        scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
    }
}
catch (Exception ex)
{
    // Manejo de errores: Si falla la inicialización (ej. configuración incorrecta), 
    // lo registramos, pero permitimos que la aplicación continúe.
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Advertencia: Falló la pre-inicialización de IExchangeRateService. Puede que la primera ejecución del Worker falle, pero la aplicación continuará.");
}

// =====================================================================
// FIN DE LA CORRECCIÓN DEL ERROR
// =====================================================================


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ESTA LÍNEA HABILITA LA CARPETA wwwroot

// =======================================================
// SOLUCIÓN DE IMÁGENES: Registra explícitamente la carpeta 'eventos'
// =======================================================
if (app.Environment.WebRootPath != null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        // Define la ruta física a la carpeta 'eventos' dentro de wwwroot
        FileProvider = new PhysicalFileProvider(
            Path.Combine(app.Environment.WebRootPath, "eventos")),

        // La URL de solicitud para acceder a esos archivos (ej: /eventos/portadas/...)
        RequestPath = "/eventos"
    });
}
// =======================================================


// AÑADIDO: Middleware necesario para aplicar la configuración de localización
app.UseRequestLocalization();

app.UseRouting();

// El orden de los middlewares es vital
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();