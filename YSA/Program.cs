using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Data.Data;
using YSA.Core.Interfaces;
using YSA.Data.Repositories;
using YSA.Core.Services;

// AÑADIDO: Necesario para manejar la configuración regional (Culture)
using System.Globalization;
using Microsoft.AspNetCore.Localization;


var builder = WebApplication.CreateBuilder(args);

// =======================================================
// INICIO: CONFIGURACIÓN GLOBAL DE CULTURA (PARA USAR DÓLARES $)
// =======================================================
var cultureInfo = new CultureInfo("en-US");

// Establece la cultura para el hilo actual (útil para código no HTTP)
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configura la cultura para todas las peticiones HTTP
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new List<CultureInfo> { cultureInfo };
    options.SupportedUICultures = new List<CultureInfo> { cultureInfo };
    // Opcional: Esto asegura que el encabezado de idioma del navegador no anule nuestra configuración.
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

// Agrega los servicios de MVC (Controladores y Vistas)
builder.Services.AddControllersWithViews();

// **Paso clave: Inyección de Dependencias de tus Repositorios y Servicios**
// Se elimina la duplicación y se añaden los nuevos servicios para el flujo de compra.
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
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IRecursoActividadRepository, RecursoActividadRepository>();
builder.Services.AddScoped<IRecursoActividadService, RecursoActividadService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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