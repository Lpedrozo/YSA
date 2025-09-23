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
using YSA.Core.Services;
using YSA.Web.Models.ViewModels;

namespace YSA.Web.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService; 

        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.GetAllAsync();

            var viewModel = new ProductosIndexViewModel
            {
                Productos = productos
            };

            return View(viewModel);
        }
    }
}